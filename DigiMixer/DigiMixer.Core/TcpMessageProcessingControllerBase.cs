using Microsoft.Extensions.Logging;

namespace DigiMixer.Core;

/// <summary>
/// A TCP controller which uses a <see cref="MessageProcessor{TMessage}"/> when
/// receiving messages.
/// </summary>
public abstract class TcpMessageProcessingControllerBase<TMessage> : TcpControllerBase where TMessage : class, IMixerMessage<TMessage>
{
    private readonly MessageProcessor<TMessage> messageProcessor;

    protected TcpMessageProcessingControllerBase(ILogger logger, string host, int port,
        int bufferSize = 65540) : base(logger, host, port)
    {
        messageProcessor = new MessageProcessor<TMessage>(ProcessMessage, bufferSize);
    }

    public async Task SendAsync(TMessage message, CancellationToken cancellationToken)
    {
        // TODO: Rent a buffer
        Memory<byte> buffer = new byte[message.Length];
        message.CopyTo(buffer.Span);
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await base.Send(buffer, cancellationToken);
    }

    protected override Task ProcessData(ReadOnlyMemory<byte> data, CancellationToken cancellationToken) =>
        messageProcessor.Process(data, cancellationToken);

    /// <summary>
    /// Processes a single received message. This is overridden in derived classes.
    /// </summary>
    /// <param name="message">The message received.</param>
    protected abstract Task ProcessMessage(TMessage message, CancellationToken cancellationToken);
}
