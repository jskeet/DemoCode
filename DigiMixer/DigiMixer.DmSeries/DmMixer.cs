using DigiMixer.Core;
using DigiMixer.DmSeries.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.DmSeries;

public static class DmMixer
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 50368, MixerApiOptions? options = null) =>
        new DmMixerApi(logger, host, port, options);
}

internal class DmMixerApi : IMixerApi
{
    // If we don't receive a keep-alive message for this long, report a problem.
    private static readonly TimeSpan KeepAliveTimeout = TimeSpan.FromSeconds(5);

    private readonly DelegatingReceiver receiver = new();
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;
    private readonly MixerApiOptions options;

    private CancellationTokenSource? cts;
    private DmClient? controlClient;
    private DmMeterClient? meterClient;

    private DateTimeOffset lastKeepAliveReceived;
    private readonly LinkedList<FullDataListener> temporaryListeners = new();

    private Task<DmMessage>? fullDataTask;

    public DmMixerApi(ILogger logger, string host, int port, MixerApiOptions? options)
    {
        this.logger = logger;
        this.host = host;
        this.port = port;
        this.options = options ?? MixerApiOptions.Default;
        lastKeepAliveReceived = DateTimeOffset.UtcNow;
    }

    public TimeSpan KeepAliveInterval { get; } = TimeSpan.FromSeconds(1);
    public IFaderScale FaderScale => DmConversions.FaderScale;

    public Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        // Propagate any existing client failures.
        if (controlClient?.ControllerStatus != ControllerStatus.Running ||
            meterClient?.ControllerStatus != ControllerStatus.Running)
        {
            logger.LogInformation("Returning unhealthy as control or meter client has failed");
            return Task.FromResult(false);
        }

        var now = DateTimeOffset.UtcNow;
        return Task.FromResult(lastKeepAliveReceived + KeepAliveTimeout > now);
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        Dispose();

        cts = new CancellationTokenSource();
        meterClient = new DmMeterClient(logger);
        //meterClient.MessageReceived += HandleMeterMessage;
        meterClient.Start();
        controlClient = new DmClient(logger, host, port, HandleControlMessage);
        await controlClient.Connect(cancellationToken);
        controlClient.Start();

        // Pretend we've seen a keep-alive message
        lastKeepAliveReceived = DateTimeOffset.UtcNow;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

        fullDataTask = RequestFullData("MMIX", "Mixing", cancellationToken);
        await fullDataTask;

        // Request live updates for channel information.
        await Send(new DmMessage("MMIX", 0x01041000, []), cancellationToken);
    }

    public async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        if (fullDataTask is null)
        {
            throw new InvalidOperationException("Cannot detect configuration until connected");
        }
        await fullDataTask;
        // TODO: Lots more! Including the stereo flags... we may not get everything we need here.
        var inputs = DmChannels.AllInputs;
        var outputs = DmChannels.AllOutputs;
        return new MixerChannelConfiguration(inputs, outputs, [StereoPair.FromLeft(ChannelId.MainOutputLeft, StereoFlags.FullyIndependent)]);
    }

    public void Dispose()
    {
        controlClient?.Dispose();
        controlClient = null;
        meterClient?.Dispose();
        meterClient = null;
        temporaryListeners.Clear();
    }

    public void RegisterReceiver(IMixerReceiver receiver) => this.receiver.RegisterReceiver(receiver);

    public async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        if (fullDataTask is null)
        {
            throw new InvalidOperationException("Cannot request all data until connected");
        }
        var message = await fullDataTask;
        // Reprocess the message
        await HandleControlMessage(message, default);
    }

    public async Task SendKeepAlive()
    {
        if (controlClient is not null && cts is not null)
        {
            await controlClient.SendAsync(DmMessages.KeepAlive, cts.Token);
        }
    }

    public async Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        // Note: this is going to be ugly if we ever support more inputs, e.g. FX as input.
        bool isStereoIn = inputId.Value is DmChannels.StereoInLeftValue or DmChannels.StereoInRightValue;
        bool isMainOut = outputId.IsMainOutput;
        string description = (isStereoIn, isMainOut) switch
        {
            (false, false) => "InputChannel/ToMix/Level",
            (false, true) => "InputChannel/Fader/Level",
            (true, false) => "StInChannel/ToMix/Level",
            (true, true) => "StInChannel/Fader/Level",
        };
        var message = new DmMessage(DmMessages.Types.Channels, 0x01110108, [
            new DmBinarySegment([0]),
            new DmTextSegment("Mixing"),
            new DmTextSegment("Mixing"),
            new DmUInt16Segment([3]),
            new DmTextSegment(description),
            new DmUInt32Segment([(uint) inputId.Value - 1, (uint) (isMainOut ? 0 : outputId.Value - 1), 0]),
            new DmUInt32Segment([0xa0]),
            new DmInt32Segment([DmConversions.FaderLevelToRaw(level)]),
        ]);
        await Send(message, cts?.Token ?? default);
    }

    public async Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        bool isMainOut = outputId.IsMainOutput;
        string description = isMainOut ? "Stereo/Fader/Level" : "Mix/Fader/Level";
        var message = new DmMessage(DmMessages.Types.Channels, 0x01110108, [
            new DmBinarySegment([0]),
            new DmTextSegment("Mixing"),
            new DmTextSegment("Mixing"),
            new DmUInt16Segment([3]),
            new DmTextSegment(description),
            new DmUInt32Segment([(uint) (isMainOut ? 0 : outputId.Value - 1), 0, 0]),
            new DmUInt32Segment([0xa0]),
            new DmInt32Segment([DmConversions.FaderLevelToRaw(level)]),
        ]);
        await Send(message, cts?.Token ?? default);
    }

    public async Task SetMuted(ChannelId channelId, bool muted)
    {
        var (description, channel) = channelId switch
        {
            { IsInput: true, Value: DmChannels.StereoInLeftValue or DmChannels.StereoInRightValue } => ("StInChannel/Fader/On", channelId.Value - 1),
            { IsInput: true } => ("InputChannel/Fader/On", channelId.Value - 1),
            { IsMainOutput: true } => ("Stereo/Fader/On", 0),
            _ => ("Mix/Fader/On", channelId.Value - 1),
        };
        var message = new DmMessage(DmMessages.Types.Channels, 0x01110108, [
            new DmBinarySegment([0]),
            new DmTextSegment("Mixing"),
            new DmTextSegment("Mixing"),
            new DmUInt16Segment([3]),
            new DmTextSegment(description),
            new DmUInt32Segment([(uint) channel, 0, 0]),
            new DmUInt32Segment([0xa0]),
            new DmUInt32Segment([muted ? 0u : 1u])
        ]);
        await Send(message, cts?.Token ?? default);
    }

    // Internal for the sake of tools
    internal async Task<DmMessage> RequestFullData(string type, string subtype, CancellationToken cancellationToken)
    {
        var listener = new FullDataListener(type, subtype, cancellationToken);
        temporaryListeners.AddLast(listener);
        // Send "tell me about your data"
        await Send(DmMessages.RequestData(type, subtype), cancellationToken);
        // Send "I don't know any checksums" (prompts full data)
        await Send(new DmMessage(type, 0x01100104, [new DmBinarySegment([00]), new DmTextSegment(subtype), DmMessages.Empty16ByteBinarySegment, DmMessages.Empty16ByteBinarySegment]), cancellationToken);
        return await listener.Task;
    }

    private async Task HandleControlMessage(DmMessage message, CancellationToken cancellationToken)
    {
        if (DmMessages.IsKeepAlive(message))
        {
            lastKeepAliveReceived = DateTimeOffset.UtcNow;
            return;
        }

        // Handle responses to full requests.
        if (message.Flags == 0x01140109 && message.Segments is
            [DmBinarySegment _, DmTextSegment { Text: string subtype }, DmTextSegment { Text: string subtype2 }, DmUInt16Segment _, DmUInt32Segment _,
            DmUInt32Segment _, DmUInt32Segment _, DmBinarySegment _, DmBinarySegment _] &&
            subtype == subtype2)
        {
            var node = temporaryListeners.First;
            while (node is not null)
            {
                var listener = node.Value;
                if (listener.Type == message.Type && listener.Subtype == subtype)
                {
                    listener.SetResult(message);
                    listener.Dispose();
                    temporaryListeners.Remove(node);
                }
                node = node.Next;
            }
            if (message.Type == DmMessages.Types.Channels)
            {
                HandleFullChannelData(new FullChannelDataMessage(message));
                fullDataTask = Task.FromResult(message);
            }
            // Acknowledge the data
            await Send(new DmMessage(message.Type, 0x01040100, []), cancellationToken);
        }

        // Fader/mute handling
        if (message.Type == DmMessages.Types.Channels && message.Flags == 0x01110108)
        {
            HandlePartialChannelData(new SingleChannelValueMessage(message));
        }
    }

    private void HandleFullChannelData(FullChannelDataMessage message)
    {
        foreach (var input in DmChannels.AllInputs)
        {
            receiver.ReceiveChannelName(input, message.GetChannelName(input));
            receiver.ReceiveMuteStatus(input, message.IsMuted(input));
            foreach (var output in DmChannels.AllOutputs)
            {
                receiver.ReceiveFaderLevel(input, output, message.GetFaderLevel(input, output));
            }
        }

        foreach (var output in DmChannels.AllOutputs)
        {
            receiver.ReceiveChannelName(output, message.GetChannelName(output));
            receiver.ReceiveMuteStatus(output, message.IsMuted(output));
            receiver.ReceiveFaderLevel(output, message.GetFaderLevel(output));
        }
    }

    private void HandlePartialChannelData(SingleChannelValueMessage message)
    {
        if (message.Description.EndsWith("/Level", StringComparison.Ordinal))
        {
            var (inputChannel, outputChannel) = message.Description switch
            {
                "InputChannel/Fader/Level" => (ChannelId.Input(message.PrimaryChannel + 1), ChannelId.MainOutputLeft),
                "InputChannel/ToMix/Level" => (ChannelId.Input(message.PrimaryChannel + 1), ChannelId.Output(message.SecondaryChannel + 1)),
                "Mix/Fader/Level" => (null, ChannelId.Output(message.PrimaryChannel + 1)),
                "Stereo/Fader/Level" => (null, ChannelId.MainOutputLeft),
                "StInChannel/Fader/Level" => (DmChannels.Stereo1Left, ChannelId.MainOutputLeft),
                "StInChannel/ToMix/Level" => (DmChannels.Stereo1Left, ChannelId.Output(message.SecondaryChannel + 1)),
                _ => ((ChannelId?) null, (ChannelId?) null)
            };
            FaderLevel level = DmConversions.RawToFaderLevel(message.Value);
            switch (inputChannel, outputChannel)
            {
                case (ChannelId input, ChannelId output):
                    receiver.ReceiveFaderLevel(input, output, level);
                    break;
                case (null, ChannelId output):
                    receiver.ReceiveFaderLevel(output, level);
                    break;
            }
        }
        else if (message.Description.EndsWith("/On", StringComparison.Ordinal))
        {
            ChannelId? channel = message.Description switch
            {
                "InputChannel/Fader/On" => ChannelId.Input(message.PrimaryChannel + 1),
                "StInChannel/Fader/On" => DmChannels.Stereo1Left,
                "Mix/Fader/On" => ChannelId.Output(message.PrimaryChannel + 1),
                "Stereo/Fader/On" => ChannelId.MainOutputLeft,
                // We ignore input/output pairs for muting, currently.
                _ => null
            };

            bool muted = message.Value == 0;
            if (channel is not null)
            {
                receiver.ReceiveMuteStatus(channel.Value, muted);
            }
        }
    }

    private async Task Send(DmMessage message, CancellationToken cancellationToken)
    {
        if (controlClient is null)
        {
            throw new InvalidOperationException("Client is not connected");
        }
        await controlClient.SendAsync(message, cancellationToken);
    }

    private class FullDataListener : IDisposable
    {
        internal string Type { get; }
        internal string Subtype { get; }

        private readonly TaskCompletionSource<DmMessage> tcs;
        private readonly CancellationTokenRegistration ctr;

        internal Task<DmMessage> Task => tcs.Task;

        internal FullDataListener(string type, string subtype, CancellationToken cancellationToken)
        {
            Type = type;
            Subtype = subtype;
            tcs = new TaskCompletionSource<DmMessage>();
            ctr = cancellationToken.Register(() => tcs.TrySetCanceled());
        }

        internal void SetResult(DmMessage message) => tcs.TrySetResult(message);

        public void Dispose() => ctr.Unregister();
    }
}