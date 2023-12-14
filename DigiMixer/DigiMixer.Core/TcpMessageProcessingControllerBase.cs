using Microsoft.Extensions.Logging;

namespace DigiMixer.Core;

/// <summary>
/// A TCP controller which uses a <see cref="MessageProcessor{TMessage}"/> when
/// receiving messages.
/// </summary>
public abstract class TcpMessageProcessingControllerBase<TMessage> : TcpControllerBase where TMessage : class
{
    private readonly MessageProcessor<TMessage> messageProcessor;

    protected TcpMessageProcessingControllerBase(ILogger logger, string host, int port,
        MessageProcessor<TMessage>.Parser messageParser, Func<TMessage, int> messageLengthExtractor, int bufferSize = 65540) : base(logger, host, port)
    {
        messageProcessor = new MessageProcessor<TMessage>(messageParser, messageLengthExtractor, ProcessMessage, bufferSize);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data) => messageProcessor.Process(data);

    /// <summary>
    /// Processes a single received message. This is overridden in derived classes.
    /// </summary>
    /// <param name="message">The message received.</param>
    protected abstract void ProcessMessage(TMessage message);
}
