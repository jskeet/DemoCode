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
        // TODO: Use an array pool
        var buffer = new byte[message.Length];
        message.CopyTo(buffer.AsSpan());
        await stream.WriteAsync(buffer, cancellationToken);
    }

    public async Task StartReading()
    {
        Memory<byte> buffer = new byte[8192];
        var messageProcessor = new MessageProcessor<UiMessage>(ProcessMessage, MaxBufferSize);

        try
        {
            while (!cts.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, cts.Token);
                if (bytesRead == 0)
                {
                    throw new InvalidOperationException("Unexpected TCP stream termination");
                }
                await messageProcessor.Process(buffer.Slice(0, bytesRead), cts.Token);
            }
        }
        catch when (cts.IsCancellationRequested)
        {
            // Swallow any errors due to disposal.
        }
    }

    private Task ProcessMessage(UiMessage message, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Processing message: {message}", message);
        }

        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        cts.Cancel();
        stream.Dispose();
    }
}
