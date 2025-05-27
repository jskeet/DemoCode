using DigiMixer.Core;
using DigiMixer.Yamaha;
using DigiMixer.Yamaha.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.TfSeries;

public static class TfMixer
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 50368, MixerApiOptions? options = null) =>
        new TfMixerApi(logger, host, port, options);
}

internal class TfMixerApi(ILogger logger, string host, int port, MixerApiOptions? options) : IMixerApi
{
    private YamahaClient? controlClient;
    private CancellationTokenSource? cts;
    private DateTimeOffset? lastKeepAliveReceived;
    private readonly MixerApiOptions options = options ?? MixerApiOptions.Default;

    public TimeSpan KeepAliveInterval => throw new NotImplementedException();

    public IFaderScale FaderScale => throw new NotImplementedException();

    public Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        Dispose();

        cts = new CancellationTokenSource();
        //meterClient = new DmMeterClient(logger);
        //meterClient.MessageReceived += HandleMeterMessage;
        //meterClient.Start();
        controlClient = new YamahaClient(logger, host, port, HandleControlMessage);
        await controlClient.Connect(cancellationToken);
        controlClient.Start();

        // Pretend we've seen a keep-alive message
        lastKeepAliveReceived = DateTimeOffset.UtcNow;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

        // fullDataTask = RequestFullData("MMIX", "Mixing", cancellationToken);
        // await fullDataTask;

        // Request live updates for channel information.
        await Send(new YamahaMessage(YamahaMessageType.MMIX, 0x01041000, []), cancellationToken);
    }

    public Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void RegisterReceiver(IMixerReceiver receiver)
    {
        throw new NotImplementedException();
    }

    public Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        throw new NotImplementedException();
    }

    public Task SendKeepAlive()
    {
        throw new NotImplementedException();
    }

    public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        throw new NotImplementedException();
    }

    public Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        throw new NotImplementedException();
    }

    public Task SetMuted(ChannelId channelId, bool muted)
    {
        throw new NotImplementedException();
    }

    private async Task HandleControlMessage(YamahaMessage message, CancellationToken cancellationToken)
    {
        var wrapped = WrappedMessage.TryParse(message);

        if (wrapped is KeepAliveMessage)
        {
            lastKeepAliveReceived = DateTimeOffset.UtcNow;
            return;
        }

        // Handle responses to full requests.
        if (message.Header == 0x01140109 && message.Segments is
            [YamahaBinarySegment _, YamahaTextSegment { Text: string subtype }, YamahaTextSegment { Text: string subtype2 }, YamahaUInt16Segment _, YamahaUInt32Segment _,
            YamahaUInt32Segment _, YamahaUInt32Segment _, YamahaBinarySegment _, YamahaBinarySegment _] &&
            subtype == subtype2)
        {
            /*
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
            if (message.Type == TfMessages.Types.Channels)
            {
                HandleFullChannelData(new FullChannelDataMessage(message));
                fullDataTask = Task.FromResult(message);
            }*/
            // Acknowledge the data
            await Send(new YamahaMessage(message.Type, 0x01040100, []), cancellationToken);
        }
    }

    private async Task Send(YamahaMessage message, CancellationToken cancellationToken)
    {
        if (controlClient is null)
        {
            throw new InvalidOperationException("Client is not connected");
        }
        await controlClient.SendAsync(message, cancellationToken);
    }
}