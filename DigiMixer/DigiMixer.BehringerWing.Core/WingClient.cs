using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.BehringerWing.Core;

public sealed class WingClient : TcpControllerBase
{
    public event EventHandler<WingToken>? AudioEngineTokenReceived;
    public event EventHandler<WingToken>? MeterTokenReceived;

    /// <summary>
    /// The amount of unprocessed data left in the buffer.
    /// </summary>
    public int UnprocessedLength { get; private set; }

    /// <summary>
    /// The total number of messages processed.
    /// </summary>
    public long MessagesProcessed { get; private set; }

    private readonly Memory<byte> sendingBuffer = new byte[32768];
    private readonly WingTokenProcessor tokenProcessor;

    public WingClient(ILogger logger, string host, int port) : base(logger, host, port)
    {
        tokenProcessor = new();
        tokenProcessor.TokenReceived += ProcessToken;
    }

    private void ProcessToken(object? sender, (WingProtocolChannel, WingToken) e)
    {
        var channel = e.Item1;
        var token = e.Item2;
        Logger.LogTrace("Received token type {type} on channel {channel}", token.Type, channel);
        switch (channel)
        {
            case WingProtocolChannel.AudioEngine:
                AudioEngineTokenReceived?.Invoke(this, token);
                break;
            case WingProtocolChannel.MeterDataRequests:
                MeterTokenReceived?.Invoke(this, token);
                break;
            default:
                // TODO: Throw an exception?
                break;
        }
    }

    // TODO: Build these into a single buffer and send in one go.
    public Task SendAudioEngineTokens(IEnumerable<WingToken> tokens, CancellationToken cancellationToken) =>
        SendTokens(WingProtocolChannel.AudioEngine, tokens, cancellationToken);

    private async Task SendTokens(WingProtocolChannel channel, IEnumerable<WingToken> tokens, CancellationToken cancellationToken)
    {
        int length = tokenProcessor.WriteTokens(channel, tokens, sendingBuffer.Span);
        await base.Send(sendingBuffer[..length], cancellationToken);
    }

    /// <summary>
    /// Synchronously processes the data from <paramref name="data"/>, retaining any data
    /// that isn't part of a message. The data may contain multiple messages, and each will be
    /// processed separately.
    /// </summary>
    protected override Task ProcessData(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        tokenProcessor.Process(data.Span);
        return Task.CompletedTask;
    }
}
