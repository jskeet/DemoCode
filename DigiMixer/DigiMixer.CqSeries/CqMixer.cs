using DigiMixer.Core;
using DigiMixer.CqSeries.Core;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DigiMixer.CqSeries;

/// <summary>
/// Mixer for Allen and Heath CQ series (currently tested with CQ20B).
/// </summary>
public class CqMixer
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 51326) =>
        new AutoReceiveMixerApi(new CqMixerApi(logger, host, port));
}

internal class CqMixerApi : IMixerApi
{
    private static readonly CqKeepAliveMessage KeepAliveMessage = new CqKeepAliveMessage();
    private static IEnumerable<ChannelId> InputChannels { get; } = Enumerable.Range(1, 24).Select(ChannelId.Input).ToList();
    private static IEnumerable<ChannelId> OutputChannels { get; } = Enumerable.Range(1, 6).Select(ChannelId.Output).Append(ChannelId.MainOutputLeft).Append(ChannelId.MainOutputRight).ToList();

    private readonly DelegatingReceiver receiver = new();
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;
    private readonly LinkedList<MessageListener> temporaryListeners = new();

    private CancellationTokenSource? cts;
    private CqControlClient? controlClient;
    private CqMeterClient? meterClient;
    private IPEndPoint? mixerUdpEndPoint;

    private MixerInfo currentMixerInfo = MixerInfo.Empty;

    internal CqMixerApi(ILogger logger, string host, int port)
    {
        this.logger = logger;
        this.host = host;
        this.port = port;
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        Dispose();

        cts = new CancellationTokenSource();
        meterClient = new CqMeterClient(logger);
        meterClient.MessageReceived += HandleMeterMessage;
        meterClient.Start();
        controlClient = new CqControlClient(logger, host, port);
        controlClient.MessageReceived += HandleControlMessage;
        await controlClient.Connect(cancellationToken);
        controlClient.Start();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
        await SendMessage(new CqUdpHandshakeMessage(meterClient.LocalUdpPort), linkedCts.Token);
        await RequestData(new CqClientInitRequestMessage(), CqMessageType.ClientInitResponse, cancellationToken);
    }

    public async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        var message = (CqFullDataResponseMessage) await RequestData(new CqFullDataRequestMessage(), CqMessageType.FullDataResponse, cancellationToken);
        return message.ToMixerChannelConfiguration();
    }

    public void RegisterReceiver(IMixerReceiver receiver) => this.receiver.RegisterReceiver(receiver);

    public async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        await SendMessage(new CqVersionRequestMessage());
        await SendMessage(new CqFullDataRequestMessage());
        await SendMessage(new CqRegularMessage(CqRegularMessage.UnitInfoRequestXyz, 0, 0, 0));

        // TODO: Unit name request
    }

    public async Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        // Propagate any existing client failures.
        if (controlClient?.ControllerStatus != ControllerStatus.Running ||
            meterClient?.ControllerStatus != ControllerStatus.Running)
        {
            return false;
        }
        await RequestData(new CqVersionRequestMessage(), CqMessageType.VersionResponse, cancellationToken);
        return true;
    }

    // TODO: Check this.
    public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(3);
    public IFaderScale FaderScale => CqConversions.FaderScale;

    public async Task SendKeepAlive()
    {
        if (meterClient is not null && cts is not null && mixerUdpEndPoint is not null)
        {
            await meterClient.SendAsync(KeepAliveMessage.RawMessage, mixerUdpEndPoint, cts.Token);
        }
    }

    public async Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        byte mappedInputId = CqChannels.ChannelIdToNetwork(inputId);
        byte mappedOutputId = CqChannels.OutputChannelIdToNetwork(outputId);

        ushort rawLevel = CqConversions.FaderLevelToRaw(level);

        var message = new CqRegularMessage(CqRegularMessage.SetFaderXyz, mappedInputId, mappedOutputId, rawLevel);

        await SendMessage(message);
    }

    // It's odd to pass the output ID in here as an input ID, but that's how faders for output channels work.
    public Task SetFaderLevel(ChannelId outputId, FaderLevel level) =>
        SetFaderLevel(outputId, ChannelId.MainOutputLeft, level);

    public async Task SetMuted(ChannelId channelId, bool muted)
    {
        byte mappedId = CqChannels.ChannelIdToNetwork(channelId);
        var message = new CqRegularMessage(CqRegularMessage.SetMuteXyz, mappedId, 0, (ushort) (muted ? 1 : 0));
        await SendMessage(message);
    }

    private async Task<CqMessage> RequestData(CqMessage requestMessage, CqMessageType expectedResponseType, CancellationToken cancellationToken)
    {
        if (controlClient is null || cts is null)
        {
            throw new InvalidOperationException("Not connected");
        }
        // TODO: thread safety...
        var listener = new MessageListener(message => message.Type == expectedResponseType, cancellationToken);
        temporaryListeners.AddLast(listener);
        await controlClient.SendAsync(requestMessage.RawMessage, cts.Token);
        return await listener.Task;
    }

    private Task SendMessage(CqMessage message) => SendMessage(message, cts?.Token ?? default);

    private async Task SendMessage(CqMessage message, CancellationToken token)
    {
        if (controlClient is not null)
        {
            await controlClient.SendAsync(message.RawMessage, token);
        }
    }

    private void HandleControlMessage(object? sender, CqRawMessage rawMessage)
    {
        var message = CqMessage.FromRawMessage(rawMessage);
        var node = temporaryListeners.First;
        while (node is not null)
        {
            if (node.Value.HandleMessage(message))
            {
                node.Value.Dispose();
                temporaryListeners.Remove(node);
            }
            node = node.Next;
        }

        switch (message)
        {
            case CqUdpHandshakeMessage handshake:
                mixerUdpEndPoint = new IPEndPoint(IPAddress.Parse(host), handshake.UdpPort);
                break;
            case CqVersionResponseMessage versionResponse:
                HandleVersionResponse(versionResponse);
                break;
            case CqFullDataResponseMessage fullDataResponse:
                HandleFullDataResponse(fullDataResponse);
                break;
            case CqRegularMessage regularMessage:
                HandleRegularMessage(regularMessage);
                break;
        }
    }

    private void HandleRegularMessage(CqRegularMessage message)
    {
        // Annoyingly, we can't make the tuples constants to switch against them.
        // We could use a ulong instead, but that's annoying.
        switch (message.XYZ)
        {
            case var xyz when xyz == CqRegularMessage.SetMuteXyz:
                HandleMute();
                break;
            case var xyz when xyz == CqRegularMessage.SetFaderXyz:
                HandleFader();
                break;
            case var xyz when xyz == CqRegularMessage.UnitInfoResponseXyz:
                HandleUnitInfoResponse();
                break;
            case var xyz when xyz == CqRegularMessage.SetNameXyz:
                HandleName();
                break;
        }

        void HandleMute()
        {
            var channelId = CqChannels.NetworkToChannelId(message.Data[3]);
            var isMuted = message.GetUInt16(5) == 1;
            receiver.ReceiveMuteStatus(channelId, isMuted);
        }

        void HandleFader()
        {
            var inputOrOutputId = CqChannels.NetworkToChannelId(message.Data[3]);
            var outputId = CqChannels.NetworkToOutputChannelId(message.Data[4]);
            var level = CqConversions.RawToFaderLevel(message.GetUInt16(5));
            if (inputOrOutputId.IsOutput)
            {
                receiver.ReceiveFaderLevel(inputOrOutputId, level);
            }
            else
            {
                receiver.ReceiveFaderLevel(inputOrOutputId, outputId, level);
            }
        }

        void HandleUnitInfoResponse()
        {
            var model = message.GetString(27, 16);
            var name = message.GetString(43, 16);
            currentMixerInfo = currentMixerInfo with { Name = name, Model = model };
            receiver.ReceiveMixerInfo(currentMixerInfo);
        }

        void HandleName()
        {
            var channelId = CqChannels.NetworkToChannelId(message.Data[4]);
            var name = message.GetString(7, 6);
            receiver.ReceiveChannelName(channelId, name);
        }
    }

    private void HandleMeterMessage(object? sender, CqRawMessage rawMessage)
    {
        var message = CqMessage.FromRawMessage(rawMessage);
        if (message is CqInputMetersMessage inputMeters)
        {
            var channelMeterPairs = new (ChannelId, MeterLevel)[16];
            for (int i = 0; i < 16; i++)
            {
                var channel = ChannelId.Input(i + 1);
                channelMeterPairs[i] = (channel, inputMeters.GetLevelPostComp(i));
            }
            receiver.ReceiveMeterLevels(channelMeterPairs);
        }
        else if (message is CqOutputMetersMessage outputMeters)
        {
            var channelMeterPairs = new (ChannelId, MeterLevel)[8];
            for (int i = 0; i < 6; i++)
            {
                var channel = ChannelId.Output(i + 1);
                channelMeterPairs[i] = (channel, outputMeters.GetLevelPostLimiter(i));
            }
            channelMeterPairs[6] = (ChannelId.MainOutputLeft, outputMeters.GetLevelPostLimiter(6));
            channelMeterPairs[7] = (ChannelId.MainOutputRight, outputMeters.GetLevelPostLimiter(7));
            receiver.ReceiveMeterLevels(channelMeterPairs);
        }
    }

    private void HandleFullDataResponse(CqFullDataResponseMessage message)
    {
        // Note that for stereo channels, we'll end up reporting values twice. That's okay.
        foreach (var input in InputChannels)
        {
            receiver.ReceiveMuteStatus(input, message.IsMuted(input));
            receiver.ReceiveChannelName(input, message.GetChannelName(input));

            foreach (var output in OutputChannels)
            {
                receiver.ReceiveFaderLevel(input, output, message.GetInputFaderLevel(input, output));
            }
        }

        foreach (var output in OutputChannels)
        {
            receiver.ReceiveMuteStatus(output, message.IsMuted(output));
            receiver.ReceiveChannelName(output, message.GetChannelName(output));
            receiver.ReceiveFaderLevel(output, message.GetOutputFaderLevel(output));
        }
    }

    private void HandleVersionResponse(CqVersionResponseMessage message)
    {
        currentMixerInfo = currentMixerInfo with { Version = message.Version };
        receiver.ReceiveMixerInfo(currentMixerInfo);
    }

    public void Dispose()
    {
        controlClient?.Dispose();
        controlClient = null;
        meterClient?.Dispose();
        meterClient = null;
        mixerUdpEndPoint = null;
        temporaryListeners.Clear();
    }

    private class MessageListener : IDisposable
    {
        private readonly TaskCompletionSource<CqMessage> tcs;
        private readonly Func<CqMessage, bool> predicate;
        private readonly CancellationTokenRegistration ctr;

        internal Task<CqMessage> Task => tcs.Task;

        internal MessageListener(Func<CqMessage, bool> predicate, CancellationToken cancellationToken)
        {
            tcs = new TaskCompletionSource<CqMessage>();
            ctr = cancellationToken.Register(() => tcs.TrySetCanceled());
            this.predicate = predicate;
        }

        internal bool HandleMessage(CqMessage message)
        {
            // If we've already cancelled the task, the listener is done.
            if (tcs.Task.IsCanceled)
            {
                return true;
            }
            if (!predicate(message))
            {
                return false;
            }
            tcs.TrySetResult(message);
            return true;
        }

        public void Dispose()
        {
            ctr.Unregister();
        }
    }
}
