using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.UiHttp;
internal sealed class UiStreamClient : IUiClient
{
    private const int MaxBufferSize = 256 * 1024; // This really should be enough.

    private readonly ILogger logger;
    private readonly Stream stream;
    private readonly CancellationTokenSource cts;

    public event EventHandler<UiMessage>? MessageReceived;

    internal UiStreamClient(ILogger logger, Stream stream)
    {
        this.logger = logger;
        this.stream = stream;
        cts = new CancellationTokenSource();
    }

    public async Task Send(UiMessage message, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Sending message: {message}", message);
        }
        var buffer = message.ToByteArray();
        // TODO: Can we handle overlapping calls?
        await stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
    }

    public async Task StartReading()
    {
        byte[] buffer = new byte[8192];
        var messageProcessor = new MessageProcessor<UiMessage>(ParseMessage, message => message.Length, ProcessMessage, MaxBufferSize);

        try
        {
            while (!cts.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, cts.Token);
                if (bytesRead == 0)
                {
                    throw new InvalidOperationException("Unexpected TCP stream termination");
                }
                messageProcessor.Process(buffer.AsSpan().Slice(0, bytesRead));
            }
        }
        catch when (cts.IsCancellationRequested)
        {
            // Swallow any errors due to disposal.
        }
    }

    private UiMessage? ParseMessage(ReadOnlySpan<byte> data)
    {
        var endOfLine = data.IndexOf((byte) '\n');
        return endOfLine == -1 ? null : UiMessage.Parse(data.Slice(0, endOfLine));
    }

    private void ProcessMessage(UiMessage message)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Processing message: {message}", message);
        }

        MessageReceived?.Invoke(this, message);
    }

    public void Dispose()
    {
        cts.Cancel();
        stream.Dispose();
    }
}
