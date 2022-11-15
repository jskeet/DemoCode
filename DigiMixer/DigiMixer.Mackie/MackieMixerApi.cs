using DigiMixer.Core;
using DigiMixer.Mackie.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection.Emit;
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
        //var meterLayout = Enumerable.Range(1, 221).SelectMany(i => new byte[] { 0, 0, 0, (byte) i });
        //await controller.SendRequest(MackieCommand.MeterLayout, new byte[] { 0, 0, 0, 1 }.Concat(meterLayout).ToArray());
        //await controller.SendRequest(MackieCommand.BroadcastControl, new byte[] { 0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x01, 0x00, 0x00, 0x5a, 0x00, 0x01 });
    }

    public Task<MixerChannelConfiguration> DetectConfiguration()
    {
        // TODO: Actually detect stuff
        var inputs = Enumerable.Range(1, 18).Select(ChannelId.Input);
        var outputs = new[] { MackieAddresses.MainOutputLeft, MackieAddresses.MainOutputRight }.Concat(Enumerable.Range(1, 6).Select(ChannelId.Output));
        var stereoPairs = new[] { new StereoPair(MackieAddresses.MainOutputLeft, MackieAddresses.MainOutputRight, StereoFlags.None) };
        var configuration = new MixerChannelConfiguration(inputs, outputs, stereoPairs);
        return Task.FromResult(configuration);
    }

    public void RegisterReceiver(IMixerReceiver receiver) =>
        this.receiver.RegisterReceiver(receiver);

    public async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        //var firmwareInfo = await SendRequest(MackieCommand.FirmwareInfo, MackiePacketBody.Empty);
        //var generalInfo = await SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 3 });
        await SendRequest(MackieCommand.ChannelInfoControl, new MackiePacketBody(new byte[] { 0, 0, 0, 6 }));
    }

    public async Task SendKeepAlive() =>
        await SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty);

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
        // TODO
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
        for (int i = 2; i < body.Length / 4; i++)
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
        }
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

    private void MapController(MackieController controller)
    {
        controller.MapBroadcastAction(HandleBroadcastPacket);
        controller.MapCommand(MackieCommand.ClientHandshake, _ => new byte[] { 0x10, 0x40, 0xf0, 0x1d, 0xbc, 0xa2, 0x88, 0x1c });
        controller.MapCommand(MackieCommand.GeneralInfo, _ => new byte[] { 0, 0, 0, 2, 0, 0, 0x40, 0 });
        // TODO: Include "if the value is 7, indicate end of data"?
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
}
