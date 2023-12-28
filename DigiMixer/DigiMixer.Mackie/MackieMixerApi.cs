using DigiMixer.Core;
using DigiMixer.Mackie.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Text;

namespace DigiMixer.Mackie;

public class MackieMixerApi : IMixerApi
{
    private delegate void ChannelValueAction(MackieMessageBody body, int chunk);

    private readonly DelegatingReceiver receiver = new();
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;

    private readonly ConcurrentDictionary<int, ChannelValueAction> channelValueActions = new();
    private readonly ConcurrentDictionary<int, Action<string>> channelNameActions = new();

    private ConcurrentDictionary<PendingChannelDataTask, PendingChannelDataTask> pendingChannelDataTasks = new();

    private MixerProfile mixerProfile;

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
        await controller.SendRequest(MackieCommand.KeepAlive, MackieMessageBody.Empty, cancellationToken);
        // Both of these are (I think) needed to get large channel value messages
        await controller.SendRequest(MackieCommand.ChannelInfoControl, new byte[8], cancellationToken);
        var handshake = await controller.SendRequest(MackieCommand.ClientHandshake, MackieMessageBody.Empty, cancellationToken);

        mixerProfile = MixerProfile.GetProfile(handshake);
        PopulateChannelValueActions();
        PopulateChannelNameActions();

        await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 2 }, cancellationToken);

        var meterAddresses = mixerProfile.InputChannels.Select(ch => ch.MeterAddress).Concat(mixerProfile.OutputChannels.Select(ch => ch.MeterAddress)).ToList();
        var meterLayout = new byte[meterAddresses.Count * 4 + 4];
        meterLayout[3] = 1;
        for (int i = 0; i < meterAddresses.Count; i++)
        {
            BinaryPrimitives.WriteInt32BigEndian(meterLayout.AsSpan().Slice(i * 4 + 4), meterAddresses[i]);
        }
        await controller.SendRequest(MackieCommand.MeterLayout, meterLayout, cancellationToken);
        await controller.SendRequest(MackieCommand.BroadcastControl, [0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x01, 0x00, 0x00, 0x5a, 0x00, 0x01], cancellationToken);
    }

    public async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        var inputs = mixerProfile.InputChannels.Select(ch => ch.Id);
        var outputs = mixerProfile.OutputChannels.Select(ch => ch.Id);

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
        var versionInfo = await SendRequest(MackieCommand.FirmwareInfo, MackieMessageBody.Empty);
        string? firmwareVersion = GetMixerFirmwareVersion(versionInfo);

        var modelInfo = await SendRequest(MackieCommand.GeneralInfo, new MackieMessageBody(new byte[] { 0, 0, 0, mixerProfile.ModelNameInfoRequest }));
        string modelName = mixerProfile.GetModelName(modelInfo);

        var generalInfo = await SendRequest(MackieCommand.GeneralInfo, new MackieMessageBody(new byte[] { 0, 0, 0, 3 }));
        string mixerName = GetMixerName(generalInfo);

        receiver?.ReceiveMixerInfo(new MixerInfo(modelName, mixerName, firmwareVersion));

        await RequestChannelData().ConfigureAwait(false);

        string? GetMixerFirmwareVersion(MackieMessage message)
        {
            var body = message.Body;

            string? firmwareVersion = null;
            // Firmware messages have an initial chunk with the XML and Mandolin version together, then
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

        string GetMixerName(MackieMessage message)
        {
            var data = message.Body.InSequentialOrder().Data;
            return Encoding.UTF8.GetString(data.Slice(4)).TrimEnd('\0');
        }
    }

    private async Task RequestChannelData(CancellationToken cancellationToken = default) =>
        await SendRequest(MackieCommand.ChannelInfoControl, new MackieMessageBody(new byte[] { 0, 0, 0, 6 }), cancellationToken).ConfigureAwait(false);

    // TODO: Check this.
    public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(3);
    public IFaderScale FaderScale => MackieConversions.FaderScale;

    public async Task SendKeepAlive() =>
        await SendRequest(MackieCommand.KeepAlive, MackieMessageBody.Empty).ConfigureAwait(false);

    public async Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        // TODO: Check if there's anything else better for this.
        await SendRequest(MackieCommand.KeepAlive, MackieMessageBody.Empty, cancellationToken);
        return true;
    }

    public async Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        var address = mixerProfile.GetFaderAddress(inputId, outputId);
        if (address is null)
        {
            return;
        }
        var body = new MackieMessageBodyBuilder(3)
            .SetInt32(0, address.Value)
            .SetInt32(1, 0x010500)
            .SetSingle(2, MackieConversions.FromFaderLevel(level))
            .Build();
        await SendRequest(MackieCommand.ChannelValues, body).ConfigureAwait(false);
        receiver.ReceiveFaderLevel(inputId, outputId, level);
    }

    public async Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        var address = mixerProfile.GetFaderAddress(outputId);
        if (address is null)
        {
            return;
        }
        var body = new MackieMessageBodyBuilder(3)
            .SetInt32(0, address.Value)
            .SetInt32(1, 0x010500)
            .SetSingle(2, MackieConversions.FromFaderLevel(level))
            .Build();
        await SendRequest(MackieCommand.ChannelValues, body).ConfigureAwait(false);
        receiver.ReceiveFaderLevel(outputId, level);
    }

    public async Task SetMuted(ChannelId channelId, bool muted)
    {
        var address = mixerProfile.GetMuteAddress(channelId);
        if (address is null)
        {
            return;
        }
        var body = new MackieMessageBodyBuilder(3)
            .SetInt32(0, address.Value)
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

    private void HandleBroadcastMessage(MackieMessage message)
    {
        var body = message.Body;

        int meterCount = mixerProfile.InputChannels.Count + mixerProfile.OutputChannels.Count;

        // 2 chunks of header, then one meter per channel.
        if (body.ChunkCount != meterCount + 2)
        {
            return;
        }

        var levels = new (ChannelId, MeterLevel)[meterCount];
        int index = 0;
        foreach (var input in mixerProfile.InputChannels)
        {
            levels[index] = (input.Id, MackieConversions.ToMeterLevel(body.GetSingle(index + 2)));
            index++;
        }
        foreach (var output in mixerProfile.OutputChannels)
        {
            levels[index] = (output.Id, MackieConversions.ToMeterLevel(body.GetSingle(index + 2)));
            index++;
        }
        receiver.ReceiveMeterLevels(levels);
    }

    private void HandleChannelValues(MackieMessage message)
    {
        var body = message.Body;
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

    private void HandleChannelNames(MackieMessage message)
    {
        var body = message.Body;
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

        var inputs = mixerProfile.InputChannels;
        var outputs = mixerProfile.OutputChannels;

        foreach (var input in inputs)
        {
            channelValueActions[input.MuteAddress] = (body, chunk) => receiver.ReceiveMuteStatus(input.Id, body.GetInt32(chunk) != 0);
            // On the DL32R, if a pair of return channels is stereo-linked, *both* channels have a value of 1
            // at their stereo link address. For all other channels, only the left channel reports a value of 1.
            // For simplicity, just ignore all right input channels.
            if ((input.Id.Value & 1) == 1)
            {
                MaybeSet(input.StereoLinkAddress, CreatePendingDataAction((pendingTask, body, chunk) => pendingTask.SetStereoLink(input.Id, body.GetInt32(chunk) == 1)));
            }
            foreach (var output in outputs)
            {
                MaybeSet(input.GetFaderAddress(output),
                    (body, chunk) => receiver.ReceiveFaderLevel(input.Id, output.Id, MackieConversions.ToFaderLevel(body.GetSingle(chunk))));
            }
        }

        foreach (var output in outputs)
        {
            MaybeSet(output.MuteAddress, (body, chunk) => receiver.ReceiveMuteStatus(output.Id, body.GetInt32(chunk) != 0));
            MaybeSet(output.FaderAddress, (body, chunk) => receiver.ReceiveFaderLevel(output.Id, MackieConversions.ToFaderLevel(body.GetSingle(chunk))));
            MaybeSet(output.StereoLinkAddress, CreatePendingDataAction((pendingTask, body, chunk) => pendingTask.SetStereoLink(output.Id, body.GetInt32(chunk) == 1)));
        }

        ChannelValueAction CreatePendingDataAction(Action<PendingChannelDataTask, MackieMessageBody, int> action) => (body, chunk) =>
        {
            foreach (var pendingDataTask in pendingChannelDataTasks.Keys)
            {
                action(pendingDataTask, body, chunk);
            }
        };

        void MaybeSet(int? address, ChannelValueAction action)
        {
            if (address is int actualAddress)
            {
                channelValueActions[actualAddress] = action;
            }
        }
    }

    private void PopulateChannelNameActions()
    {
        channelNameActions.Clear();

        foreach (var input in mixerProfile.InputChannels)
        {
            channelNameActions[input.NameIndex] = name => receiver.ReceiveChannelName(input.Id, EmptyToNull(name));
        }

        foreach (var output in mixerProfile.OutputChannels)
        {
            if (output.NameIndex is int nameIndex)
            {
                channelNameActions[nameIndex] = name =>
                {
                    string? effectiveName = EmptyToNull(name);
                    if (effectiveName is null && output.Id.IsMainOutput)
                    {
                        effectiveName = "Main";
                    }
                    receiver.ReceiveChannelName(output.Id, effectiveName);
                };
            }
        }

        string? EmptyToNull(string text) => text == "" ? null : text;
    }

    private void MaybeCompleteChannelInfo(MackieMessage message)
    {
        var body = message.Body;
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
        controller.MapBroadcastAction(HandleBroadcastMessage);
        // TODO: Ideally we'd provide the MAC address instead of zeroes here... but getting the right
        // address is fiddly at best. Better to provide zeroes than a wrong one.
        controller.MapCommand(MackieCommand.ClientHandshake, _ => new byte[] { 0x10, 0x40, 0, 0, 0, 0, 0, 0 });
        controller.MapCommand(MackieCommand.GeneralInfo, _ => new byte[] { 0, 0, 0, 2, 0, 0, 0x40, 0 });
        controller.MapCommandAction(MackieCommand.ChannelInfoControl, MaybeCompleteChannelInfo);
        controller.MapCommand(MackieCommand.ChannelInfoControl, message => new MackieMessageBody(message.Body.Data.Slice(0, 4)));
        controller.MapCommandAction(MackieCommand.ChannelValues, HandleChannelValues);
        controller.MapCommandAction(MackieCommand.ChannelNames, HandleChannelNames);
    }

    public async Task<MackieMessage> SendRequest(MackieCommand command, MackieMessageBody body, CancellationToken cancellationToken = default) =>
        controller is null
            ? new MackieMessage(1, MackieMessageType.Response, command, MackieMessageBody.Empty)
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
