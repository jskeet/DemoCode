using DigiMixer.Core;
using DigiMixer.Mackie.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Text;

namespace DigiMixer.Mackie;

public class MackieMixerApi : IMixerApi
{
    private delegate void ChannelValueAction(MackiePacketBody body, int chunk);

    private readonly DelegatingReceiver receiver = new();
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;

    private readonly ConcurrentDictionary<int, ChannelValueAction> channelValueActions = new();
    private readonly ConcurrentDictionary<int, Action<string>> channelNameActions = new();

    private ConcurrentDictionary<PendingChannelDataTask, PendingChannelDataTask> pendingChannelDataTasks = new();

    private IMixerProfile mixerProfile;

    private MackieController? controller;

    public MackieMixerApi(ILogger? logger, string host, int port = 50001)
    {
        this.logger = logger ?? NullLogger.Instance;
        this.host = host;
        this.port = port;
        mixerProfile = NullProfile.Instance;
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        Dispose();

        controller = new MackieController(logger, host, port);
        MapController(controller);
        await controller.Connect(cancellationToken);
        controller.Start();

        // Initialization handshake
        await controller.SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty, cancellationToken);
        // Both of these are (I think) needed to get large channel value packets
        await controller.SendRequest(MackieCommand.ChannelInfoControl, new byte[8], cancellationToken);
        var handshake = await controller.SendRequest(MackieCommand.ClientHandshake, MackiePacketBody.Empty, cancellationToken);

        mixerProfile = IMixerProfile.GetProfile(handshake);
        PopulateChannelValueActions();
        PopulateChannelNameActions();

        await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 2 }, cancellationToken);

        var inputMeters = Enumerable.Range(1, mixerProfile.InputChannelCount).Select(input => (input - 1) * 7 + 0x22);
        var mainMeters = new int[] { 0xbe, 0xbf };
        var auxMeters = Enumerable.Range(1, mixerProfile.AuxChannelCount).Select(aux => (aux - 1) * 4 + 0xc6);
        var meterLayout = inputMeters.Concat(mainMeters).Concat(auxMeters).SelectMany(i => new byte[] { 0, 0, 0, (byte) i });
        //var meterLayout = Enumerable.Range(1, 221).SelectMany(i => new byte[] { 0, 0, 0, (byte) i });
        await controller.SendRequest(MackieCommand.MeterLayout,
            new byte[] { 0, 0, 0, 1 }.Concat(meterLayout).ToArray(),
            cancellationToken);
        await controller.SendRequest(MackieCommand.BroadcastControl,
            new byte[] { 0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x01, 0x00, 0x00, 0x5a, 0x00, 0x01 },
            cancellationToken);
    }

    public async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        var inputs = Enumerable.Range(1, mixerProfile.InputChannelCount).Select(ChannelId.Input);
        var outputs = new[] { ChannelId.MainOutputLeft, ChannelId.MainOutputRight }.Concat(Enumerable.Range(1, 6).Select(ChannelId.Output));

        var pendingTask = PendingChannelDataTask.Start(pendingChannelDataTasks, cancellationToken);
        await RequestChannelData(cancellationToken).ConfigureAwait(false);
        var pendingData = await pendingTask.ConfigureAwait(false);

        var stereoPairs = pendingData.GetStereoLinks()
            .Append(ChannelId.MainOutputLeft)
            .Select(link => new StereoPair(link, link.WithValue(link.Value + 1), StereoFlags.None));
        return new MixerChannelConfiguration(inputs, outputs, stereoPairs);
    }

    public void RegisterReceiver(IMixerReceiver receiver) =>
        this.receiver.RegisterReceiver(receiver);

    public async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        var versionInfo = await SendRequest(MackieCommand.FirmwareInfo, MackiePacketBody.Empty);
        string? firmwareVersion = GetMixerFirmwareVersion(versionInfo);

        var modelInfo = await SendRequest(MackieCommand.GeneralInfo, new MackiePacketBody(new byte[] { 0, 0, 0, mixerProfile.ModelNameInfoRequest }));
        string modelName = mixerProfile.GetModelName(modelInfo);

        var generalInfo = await SendRequest(MackieCommand.GeneralInfo, new MackiePacketBody(new byte[] { 0, 0, 0, 3 }));
        string mixerName = GetMixerName(generalInfo);

        receiver?.ReceiveMixerInfo(new MixerInfo(modelName, mixerName, firmwareVersion));

        await RequestChannelData().ConfigureAwait(false);

        string? GetMixerFirmwareVersion(MackiePacket packet)
        {
            var body = packet.Body;

            string? firmwareVersion = null;
            // Firmware packets have an initial chunk with the XML and Mandolin version together, then
            // key/value pairs of chunks. The mixer firmware version has a key of 2.
            for (int chunk = 1; chunk < body.ChunkCount - 1; chunk += 2)
            {
                uint key = body.GetUInt32(chunk);
                if (key == 2)
                {
                    uint value = body.GetUInt32(chunk + 1);
                    firmwareVersion = "0x" + value.ToString("x8");
                }
            }
            return firmwareVersion;
        }

        string GetMixerName(MackiePacket packet)
        {
            var data = packet.Body.InSequentialOrder().Data;
            return Encoding.UTF8.GetString(data.Slice(4)).TrimEnd('\0');
        }
    }

    private async Task RequestChannelData(CancellationToken cancellationToken = default) =>
        await SendRequest(MackieCommand.ChannelInfoControl, new MackiePacketBody(new byte[] { 0, 0, 0, 6 }), cancellationToken).ConfigureAwait(false);

    // TODO: Check this.
    public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(3);

    public async Task SendKeepAlive() =>
        await SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty).ConfigureAwait(false);

    public async Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        // TODO: Check if there's anything else better for this.
        await SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty, cancellationToken);
        return true;
    }

    public async Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        var address = mixerProfile.GetFaderAddress(inputId, outputId);
        var body = new MackiePacketBodyBuilder(3)
            .SetInt32(0, address)
            .SetInt32(1, 0x010500)
            .SetSingle(2, MackieConversions.FromFaderLevel(level))
            .Build();
        await SendRequest(MackieCommand.ChannelValues, body).ConfigureAwait(false);
        receiver.ReceiveFaderLevel(inputId, outputId, level);
    }

    public async Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        var address = mixerProfile.GetFaderAddress(outputId);
        var body = new MackiePacketBodyBuilder(3)
            .SetInt32(0, address)
            .SetInt32(1, 0x010500)
            .SetSingle(2, MackieConversions.FromFaderLevel(level))
            .Build();
        await SendRequest(MackieCommand.ChannelValues, body).ConfigureAwait(false);
        receiver.ReceiveFaderLevel(outputId, level);
    }

    public async Task SetMuted(ChannelId channelId, bool muted)
    {
        var address = mixerProfile.GetMuteAddress(channelId);
        var body = new MackiePacketBodyBuilder(3)
            .SetInt32(0, address)
            .SetInt32(1, 0x010500)
            .SetInt32(2, muted ? 1 : 0)
            .Build();
        await SendRequest(MackieCommand.ChannelValues, body).ConfigureAwait(false);
        receiver.ReceiveMuteStatus(channelId, muted);
    }

    public void Dispose()
    {
        controller?.Dispose();
        controller = null;
    }

    private void HandleBroadcastPacket(MackiePacket packet)
    {
        var body = packet.Body;

        int meterCount = mixerProfile.InputChannelCount + mixerProfile.AuxChannelCount + 2;

        // 2 chunks of header, then the following chunks values:
        // - Input channels (pre-fader)
        // - Main L/R (post-fader)
        // - Aux channels (post-fader)
        if (body.ChunkCount != meterCount)
        {
            return;
        }

        var levels = new (ChannelId, MeterLevel)[meterCount];

        int index = 0;
        for (int i = 1; i <= mixerProfile.InputChannelCount; i++)
        {
            ChannelId inputId = ChannelId.Input(i);
            levels[index] = (inputId, MackieConversions.ToMeterLevel(body.GetSingle(index + 2)));
            index++;
        }
        levels[index] = (ChannelId.MainOutputLeft, MackieConversions.ToMeterLevel(body.GetSingle(index + 2)));
        index++;
        levels[index] = (ChannelId.MainOutputRight, MackieConversions.ToMeterLevel(body.GetSingle(index + 2)));
        index++;
        for (int i = 1; i <= mixerProfile.AuxChannelCount; i++)
        {
            ChannelId outputId = ChannelId.Output(i);
            levels[index] = (outputId, MackieConversions.ToMeterLevel(body.GetSingle(index + 2)));
            index++;
        }
        receiver.ReceiveMeterLevels(levels);
    }

    private void HandleChannelValues(MackiePacket packet)
    {
        var body = packet.Body;
        if (body.Length < 8)
        {
            return;
        }
        uint chunk1 = body.GetUInt32(1);
        // TODO: Handle other value types.
        if ((chunk1 & 0xff00) != 0x0500)
        {
            return;
        }
        int start = body.GetInt32(0);
        for (int i = 0; i < body.ChunkCount - 2; i++)
        {
            int address = start + i;

            if (channelValueActions.TryGetValue(address, out var action))
            {
                action(body, i + 2);
            }
        }
    }

    private void HandleChannelNames(MackiePacket packet)
    {
        var body = packet.Body;
        if (body.Length < 8)
        {
            return;
        }
        uint chunk1 = body.GetUInt32(1);
        // TODO: Handle other name types.
        if ((chunk1 & 0xff00) != 0x0500)
        {
            return;
        }
        int start = body.GetInt32(0);
        int count = (int) (chunk1 >> 16);
        string allNames = Encoding.UTF8.GetString(body.InSequentialOrder().Data.Slice(8).ToArray());

        string[] names = allNames.Split('\0');
        for (int i = 0; i < count; i++)
        {
            int address = start + i;
            if (channelNameActions.TryGetValue(address, out var action))
            {
                action(names[i]);
            }
        }
    }

    private void PopulateChannelValueActions()
    {
        channelValueActions.Clear();
        
        var inputIds = Enumerable.Range(1, mixerProfile.InputChannelCount).Select(ChannelId.Input).ToList();
        var outputIds = Enumerable.Range(1, mixerProfile.AuxChannelCount).Select(ChannelId.Output).Append(ChannelId.MainOutputLeft).ToList();

        foreach (var inputId in inputIds)
        {
            channelValueActions[mixerProfile.GetMuteAddress(inputId)] = (body, chunk) => receiver.ReceiveMuteStatus(inputId, body.GetInt32(chunk) != 0);
            channelValueActions[mixerProfile.GetStereoLinkAddress(inputId)] = CreatePendingDataAction((pendingTask, body, chunk) => pendingTask.SetStereoLink(inputId, body.GetInt32(chunk) == 1));
            foreach (var outputId in outputIds)
            {
                channelValueActions[mixerProfile.GetFaderAddress(inputId, outputId)] = (body, chunk) =>
                    receiver.ReceiveFaderLevel(inputId, outputId, MackieConversions.ToFaderLevel(body.GetSingle(chunk)));
            }
        }

        foreach (var outputId in outputIds)
        {
            channelValueActions[mixerProfile.GetMuteAddress(outputId)] = (body, chunk) => receiver.ReceiveMuteStatus(outputId, body.GetInt32(chunk) != 0);
            channelValueActions[mixerProfile.GetFaderAddress(outputId)] = (body, chunk) =>
                receiver.ReceiveFaderLevel(outputId, MackieConversions.ToFaderLevel(body.GetSingle(chunk)));
            channelValueActions[mixerProfile.GetStereoLinkAddress(outputId)] = CreatePendingDataAction((pendingTask, body, chunk) => pendingTask.SetStereoLink(outputId, body.GetInt32(chunk) == 1));
        }

        ChannelValueAction CreatePendingDataAction(Action<PendingChannelDataTask, MackiePacketBody, int> action) => (body, chunk) =>
        {
            foreach (var pendingDataTask in pendingChannelDataTasks.Keys)
            {
                action(pendingDataTask, body, chunk);
            }
        };
    }

    private void PopulateChannelNameActions()
    {
        channelNameActions.Clear();

        var inputIds = Enumerable.Range(1, mixerProfile.InputChannelCount).Select(ChannelId.Input).ToList();
        var outputIds = Enumerable.Range(1, mixerProfile.AuxChannelCount).Select(ChannelId.Output).Append(ChannelId.MainOutputLeft).ToList();
        var allIds = inputIds.Concat(outputIds);
        foreach (var id in allIds)
        {
            channelNameActions[mixerProfile.GetNameAddress(id)] = name =>
            {
                string? effectiveName = name == "" ? null : name;
                // TODO: Work out a neater place to put this.
                if (effectiveName is null && id.IsMainOutput)
                {
                    effectiveName = "Main";
                }
                receiver.ReceiveChannelName(id, effectiveName);
            };
        }
    }

    private void MaybeCompleteChannelInfo(MackiePacket packet)
    {
        var body = packet.Body;
        if (body.Length < 1)
        {
            return;
        }
        if (body.GetInt32(0) != 7)
        {
            return;
        }
        foreach (var pendingDataTask in pendingChannelDataTasks.Keys)
        {
            pendingDataTask.SignalCompletion();
        }
    }

    private void MapController(MackieController controller)
    {
        controller.MapBroadcastAction(HandleBroadcastPacket);
        // TODO: Ideally we'd provide the MAC address instead of zeroes here... but getting the right
        // address is fiddly at best. Better to provide zeroes than a wrong one.
        controller.MapCommand(MackieCommand.ClientHandshake, _ => new byte[] { 0x10, 0x40, 0, 0, 0, 0, 0, 0 });
        controller.MapCommand(MackieCommand.GeneralInfo, _ => new byte[] { 0, 0, 0, 2, 0, 0, 0x40, 0 });
        controller.MapCommandAction(MackieCommand.ChannelInfoControl, MaybeCompleteChannelInfo);
        controller.MapCommand(MackieCommand.ChannelInfoControl, packet => new MackiePacketBody(packet.Body.Data.Slice(0, 4)));
        controller.MapCommandAction(MackieCommand.ChannelValues, HandleChannelValues);
        controller.MapCommandAction(MackieCommand.ChannelNames, HandleChannelNames);
    }

    public async Task<MackiePacket> SendRequest(MackieCommand command, MackiePacketBody body, CancellationToken cancellationToken = default) =>
        controller is null
            ? new MackiePacket(1, MackiePacketType.Response, command, MackiePacketBody.Empty)
            : await controller.SendRequest(command, body, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// TODO: Rename this, maybe handle it differently... it's all a bit odd, basically.
    /// This is used when detecting configuration. We issue a "give me all the data" command, and then wait for the mixer to send "all done".
    /// That's fiddly.
    /// </summary>
    private class PendingChannelDataTask
    {
        private readonly TaskCompletionSource<PendingChannelDataTask> tcs;
        private readonly CancellationTokenRegistration cancellationTokenRegistration;
        // TODO: Use a different collection type. Most concurrent collections don't have a way of removing an item.
        private readonly ConcurrentDictionary<PendingChannelDataTask, PendingChannelDataTask> parentCollection;

        private readonly ConcurrentDictionary<ChannelId, bool> stereoLinks;

        private PendingChannelDataTask(ConcurrentDictionary<PendingChannelDataTask, PendingChannelDataTask> parentCollection, CancellationToken cancellationToken)
        {
            this.parentCollection = parentCollection;
            stereoLinks = new();
            tcs = new TaskCompletionSource<PendingChannelDataTask>();
            cancellationTokenRegistration = cancellationToken.Register(SignalCancellation);
            parentCollection[this] = this;
        }

        public static Task<PendingChannelDataTask> Start(ConcurrentDictionary<PendingChannelDataTask, PendingChannelDataTask> parentCollection, CancellationToken cancellationToken)
        {
            var obj = new PendingChannelDataTask(parentCollection, cancellationToken);
            return obj.tcs.Task;
        }

        private void SignalCancellation()
        {
            tcs.TrySetCanceled();
            cancellationTokenRegistration.Unregister();
            parentCollection.TryRemove(this, out _);
        }

        public void SignalCompletion()
        {
            tcs.TrySetResult(this);
            cancellationTokenRegistration.Unregister();
            parentCollection.TryRemove(this, out _);
        }

        public void SetStereoLink(ChannelId channelId, bool stereo) =>
            stereoLinks[channelId] = stereo;

        public IEnumerable<ChannelId> GetStereoLinks() => stereoLinks.Where(pair => pair.Value).Select(pair => pair.Key).ToList();
    }
}
