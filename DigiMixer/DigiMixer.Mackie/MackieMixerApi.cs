using DigiMixer.Core;
using DigiMixer.Mackie.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace DigiMixer.Mackie;

public class MackieMixerApi : IMixerApi
{
    private delegate void ChannelValueAction(MackiePacketBody body, int chunk);

    private readonly DelegatingReceiver receiver = new();
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;

    private readonly Dictionary<int, ChannelValueAction> channelValueActions;
    private readonly Dictionary<int, Action<string>> channelNameActions;

    private ConcurrentDictionary<PendingChannelDataTask, PendingChannelDataTask> pendingChannelDataTasks = new();

    private MackieController? controller;
    private Task? controllerTask;

    public MackieMixerApi(ILogger? logger, string host, int port = 50001)
    {
        this.logger = logger ?? NullLogger.Instance;
        this.host = host;
        this.port = port;
        controllerTask = Task.CompletedTask;

        channelValueActions = PopulateChannelValueActions();
        channelNameActions = PopulateChannelNameActions();
    }

    public async Task Connect()
    {
        Dispose();

        controller = new MackieController(logger, host, port);
        MapController(controller);
        controllerTask = controller.Start();

        // Initialization handshake
        await controller.SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty);
        // Both of these are (I think) needed to get large channel value packets
        await controller.SendRequest(MackieCommand.ChannelInfoControl, new byte[8]);
        await controller.SendRequest((MackieCommand) 3, MackiePacketBody.Empty);
        await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 2 });

        var inputMeters = Enumerable.Range(1, 18).Select(input => (input - 1) * 7 + 0x22);
        var mainMeters = new int[] { 0xbe, 0xbf };
        var auxMeters = Enumerable.Range(1, 6).Select(aux => (aux - 1) * 4 + 0xc6);
        var meterLayout = inputMeters.Concat(mainMeters).Concat(auxMeters).SelectMany(i => new byte[] { 0, 0, 0, (byte) i });
        //var meterLayout = Enumerable.Range(1, 221).SelectMany(i => new byte[] { 0, 0, 0, (byte) i });
        await controller.SendRequest(MackieCommand.MeterLayout, new byte[] { 0, 0, 0, 1 }.Concat(meterLayout).ToArray());
        await controller.SendRequest(MackieCommand.BroadcastControl, new byte[] { 0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x01, 0x00, 0x00, 0x5a, 0x00, 0x01 });
    }

    public async Task<MixerChannelConfiguration> DetectConfiguration()
    {
        var inputs = Enumerable.Range(1, 18).Select(ChannelId.Input);
        var outputs = new[] { MackieAddresses.MainOutputLeft, MackieAddresses.MainOutputRight }.Concat(Enumerable.Range(1, 6).Select(ChannelId.Output));

        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token;
        var pendingTask = PendingChannelDataTask.Start(pendingChannelDataTasks, cancellationToken);
        await RequestChannelData(cancellationToken).ConfigureAwait(false);
        var pendingData = await pendingTask.ConfigureAwait(false);

        var stereoPairs = pendingData.GetStereoLinks()
            .Append(MackieAddresses.MainOutputLeft)
            .Select(link => new StereoPair(link, link.WithValue(link.Value + 1), StereoFlags.None));
        return new MixerChannelConfiguration(inputs, outputs, stereoPairs);
    }

    public void RegisterReceiver(IMixerReceiver receiver) =>
        this.receiver.RegisterReceiver(receiver);

    public async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        //var firmwareInfo = await SendRequest(MackieCommand.FirmwareInfo, MackiePacketBody.Empty);
        //var generalInfo = await SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 3 });
        await RequestChannelData().ConfigureAwait(false);
    }

    private async Task RequestChannelData(CancellationToken cancellationToken = default) =>
        await SendRequest(MackieCommand.ChannelInfoControl, new MackiePacketBody(new byte[] { 0, 0, 0, 6 }), cancellationToken).ConfigureAwait(false);

    public async Task SendKeepAlive() =>
        await SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty).ConfigureAwait(false);

    public async Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        var address = MackieAddresses.GetFaderAddress(inputId, outputId);
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
        var address = MackieAddresses.GetFaderAddress(outputId);
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
        var address = MackieAddresses.GetMuteAddress(channelId);
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
        controllerTask = null;
    }

    private void HandleBroadcastPacket(MackiePacket packet)
    {
        var body = packet.Body;

        // 2 chunks of header, then 26 chunks values:
        // - Input channels 1-18 (pre-fader)
        // - Main L/R (post-fader)
        // - Aux 1-6 (post-fader)
        if (body.ChunkCount != 2 + 26)
        {
            return;
        }

        var levels = new (ChannelId, MeterLevel)[18 + 2 + 6];

        int index = 0;
        for (int i = 1; i <= 18; i++)
        {
            ChannelId inputId = ChannelId.Input(i);
            levels[index] = (inputId, MackieConversions.ToMeterLevel(body.GetSingle(index + 2)));
            index++;
        }
        levels[index] = (MackieAddresses.MainOutputLeft, MackieConversions.ToMeterLevel(body.GetSingle(index + 2)));
        index++;
        levels[index] = (MackieAddresses.MainOutputRight, MackieConversions.ToMeterLevel(body.GetSingle(index + 2)));
        index++;
        for (int i = 1; i <= 6; i++)
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
        int chunk1 = body.GetInt32(1);
        // TODO: Handle other value types.
        if ((chunk1 & 0xff00) != 0x0500)
        {
            return;
        }
        int start = body.GetInt32(0);
        for (int i = 2; i < body.ChunkCount; i++)
        {
            int address = start - 2 + i;

            if (channelValueActions.TryGetValue(address, out var action))
            {
                action(body, i);
            }
        }
    }

    private void HandleChannelNames(MackiePacket packet)
    {
        if (packet.Body.Length < 8)
        {
            return;
        }
        int start = packet.Body.GetInt32(0);
        string allNames = Encoding.ASCII.GetString(packet.Body.InSequentialOrder().Data.Slice(8).ToArray());

        string[] names = allNames.Split('\0');
        for (int i = 0; i < names.Length; i++)
        {
            int address = start + i;
            if (channelNameActions.TryGetValue(address, out var action))
            {
                action(names[i]);
            }
        }
    }

    private Dictionary<int, ChannelValueAction> PopulateChannelValueActions()
    {
        var dictionary = new Dictionary<int, ChannelValueAction>();

        var inputIds = Enumerable.Range(1, 18).Select(ChannelId.Input).ToList();
        var outputIds = Enumerable.Range(1, 6).Select(ChannelId.Output).Append(MackieAddresses.MainOutputLeft).ToList();

        foreach (var inputId in inputIds)
        {
            dictionary.Add(MackieAddresses.GetMuteAddress(inputId), (body, chunk) => receiver.ReceiveMuteStatus(inputId, body.GetInt32(chunk) != 0));
            dictionary.Add(MackieAddresses.GetStereoLinkAddress(inputId), CreatePendingDataAction((pendingTask, body, chunk) => pendingTask.SetStereoLink(inputId, body.GetInt32(chunk) == 1)));
            foreach (var outputId in outputIds)
            {
                dictionary.Add(MackieAddresses.GetFaderAddress(inputId, outputId), (body, chunk) =>
                    receiver.ReceiveFaderLevel(inputId, outputId, MackieConversions.ToFaderLevel(body.GetSingle(chunk))));
            }
        }

        foreach (var outputId in outputIds)
        {
            dictionary.Add(MackieAddresses.GetMuteAddress(outputId), (body, chunk) => receiver.ReceiveMuteStatus(outputId, body.GetInt32(chunk) != 0));
            dictionary.Add(MackieAddresses.GetFaderAddress(outputId), (body, chunk) =>
                receiver.ReceiveFaderLevel(outputId, MackieConversions.ToFaderLevel(body.GetSingle(chunk))));
            dictionary.Add(MackieAddresses.GetStereoLinkAddress(outputId), CreatePendingDataAction((pendingTask, body, chunk) => pendingTask.SetStereoLink(outputId, body.GetInt32(chunk) == 1)));
        }

        ChannelValueAction CreatePendingDataAction(Action<PendingChannelDataTask, MackiePacketBody, int> action) => (body, chunk) =>
        {
            foreach (var pendingDataTask in pendingChannelDataTasks.Keys)
            {
                action(pendingDataTask, body, chunk);
            }
        };

        return dictionary;
    }

    private Dictionary<int, Action<string>> PopulateChannelNameActions()
    {
        var dictionary = new Dictionary<int, Action<string>>();

        var inputIds = Enumerable.Range(1, 18).Select(ChannelId.Input).ToList();
        var outputIds = Enumerable.Range(1, 6).Select(ChannelId.Output).Append(MackieAddresses.MainOutputLeft).ToList();
        var allIds = inputIds.Concat(outputIds);
        foreach (var id in allIds)
        {
            dictionary.Add(MackieAddresses.GetNameAddress(id), name =>
            {
                string? effectiveName = name == "" ? null : name;
                // TODO: Work out a neater place to put this.
                if (effectiveName is null && id == MackieAddresses.MainOutputLeft)
                {
                    effectiveName = "Main";
                }
                receiver.ReceiveChannelName(id, effectiveName);
            });
        }
        return dictionary;
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
        controller.MapCommand(MackieCommand.ClientHandshake, _ => new byte[] { 0x10, 0x40, 0xf0, 0x1d, 0xbc, 0xa2, 0x88, 0x1c });
        controller.MapCommand(MackieCommand.GeneralInfo, _ => new byte[] { 0, 0, 0, 2, 0, 0, 0x40, 0 });
        controller.MapCommandAction(MackieCommand.ChannelInfoControl, MaybeCompleteChannelInfo);
        controller.MapCommand(MackieCommand.ChannelInfoControl, packet => new MackiePacketBody(packet.Body.Data.Slice(0, 4)));
        controller.MapCommandAction(MackieCommand.ChannelValues, HandleChannelValues);
        controller.MapCommandAction(MackieCommand.ChannelNames, HandleChannelNames);
    }

    private Task<MackiePacket> SendRequest(MackieCommand command, byte[] body, CancellationToken cancellationToken = default) =>
        SendRequest(command, new MackiePacketBody(body), cancellationToken);

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
