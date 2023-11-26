using DigiMixer.Core;
using DigiMixer.CqSeries.Core;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace DigiMixer.CqSeries;

public class CqMixer
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 51326) =>
        new AutoReceiveMixerApi(new CqMixerApi(logger, host, port));
}

internal class CqMixerApi : IMixerApi
{
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
        await controlClient.SendAsync(new CqHandshakeMessage(meterClient.LocalUdpPort), linkedCts.Token);
        // TODO: Wait until we've had a reply?
        // TODO: Type 12 and 13?
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
        await RequestData(new CqVersionRequestMessage(), CqMessageType.VersionRequest, cancellationToken);
        return true;
    }

    // TODO: Check this.
    public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(3);

    public async Task SendKeepAlive()
    {
        if (meterClient is not null && cts is not null && mixerUdpEndPoint is not null)
        {
            await meterClient.SendKeepAliveAsync(mixerUdpEndPoint, cts.Token);
        }
    }

    public async Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        byte mappedInputId = inputId.Value switch
        {
            int ch when ch < 17 => (byte) (ch - 1),
            17 or 18 => 0x18,
            19 or 20 => 0x1A,
            21 or 22 => 0x1C,
            23 or 24 => 0x1E,
            _ => throw new InvalidOperationException($"Unable to set fader level for {inputId}")
        };
        byte mappedOutputId = outputId switch
        {
            { Value: int ch } when ch < 7 => (byte) (0x7 + ch),
            { IsMainOutput: true } => 0x10,
            _ => throw new InvalidOperationException($"Unable to set fader level for {outputId}")
        };

        ushort rawLevel = CqConversions.FaderLevelToRaw(level);

        var message = new CqRegularMessage(CqMessageFormat.FixedLength8, 6, 6, 14,
            [mappedInputId, mappedOutputId, (byte) (rawLevel & 0xff), (byte) (rawLevel >> 8)]);

        await SendMessage(message);
    }

    public async Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        byte mappedOutputId = outputId switch
        {
            { Value: int ch } when ch < 7 => (byte) (0x2f + ch),
            { IsMainOutput: true } => 0x38,
            _ => throw new InvalidOperationException($"Unable to set fader level for {outputId}")
        };
        ushort rawLevel = CqConversions.FaderLevelToRaw(level);

        var message = new CqRegularMessage(CqMessageFormat.FixedLength8, 6, 6, 14,
            [mappedOutputId, 0x10, (byte) (rawLevel & 0xff), (byte) (rawLevel >> 8)]);
        await SendMessage(message);
    }

    public async Task SetMuted(ChannelId channelId, bool muted)
    {
        byte mappedId = channelId switch
        {
            { IsInput: true, Value: int ch } when ch < 17 => (byte) (ch - 1),
            { IsInput: true, Value: 17 or 18 } => 0x18,
            { IsInput: true, Value: 19 or 20 } => 0x1A,
            { IsInput: true, Value: 21 or 22 } => 0x1C,
            { IsInput: true, Value: 23 or 24 } => 0x1E,
            { IsOutput: true, Value: int ch } when ch < 7 => (byte) (0x2f + ch),
            { IsMainOutput: true } => 0x38,
            _ => throw new InvalidOperationException($"Unable to set mute for {channelId}")
        };

        var message = new CqRegularMessage(CqMessageFormat.FixedLength8, 6, 6, 12, [mappedId, 0, (byte) (muted ? 1 : 0), 0]);
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
        await controlClient.SendAsync(requestMessage, cts.Token);
        return await listener.Task;
    }

    private async Task SendMessage(CqMessage message)
    {
        if (controlClient is not null)
        {
            await controlClient.SendAsync(message, cts?.Token ?? default);
        }
    }

    private void HandleControlMessage(object? sender, CqMessage message)
    {
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
            case CqHandshakeMessage handshake:
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
    }

    private void HandleMeterMessage(object? sender, CqMessage message)
    {
        /* TODO */
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
        // Note: the model is probably encoded in the first two bytes, but that's hard to check...
        var data = message.Data;
        int firmwareMajor = data[3];
        int firmwareMinor = data[2];
        ushort revision = MemoryMarshal.Read<ushort>(data.Slice(4, 2));
        currentMixerInfo = currentMixerInfo with { Model = "Qu-???", Version = $"{firmwareMajor}.{firmwareMinor} rev {revision}" };
        receiver.ReceiveMixerInfo(currentMixerInfo);
    }

    public void Dispose()
    {
        controlClient?.Dispose();
        controlClient = null;
        meterClient?.Dispose();
        meterClient = null;
        mixerUdpEndPoint = null;
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
