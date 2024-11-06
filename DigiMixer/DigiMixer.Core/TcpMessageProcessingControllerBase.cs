using Microsoft.Extensions.Logging;
using System.Buffers;

namespace DigiMixer.Core;

/// <summary>
/// A TCP controller which uses a <see cref="MessageProcessor{TMessage}"/> when
/// receiving messages.
/// </summary>
public abstract class TcpMessageProcessingControllerBase<TMessage> : TcpControllerBase where TMessage : class, IMixerMessage<TMessage>
{
    private readonly MemoryPool<byte> SendingPool = MemoryPool<byte>.Shared;

    private readonly MessageProcessor<TMessage> messageProcessor;

    protected TcpMessageProcessingControllerBase(ILogger logger, string host, int port,
        int bufferSize = 65540) : base(logger, host, port)
    {
        messageProcessor = new MessageProcessor<TMessage>(ProcessMessage, bufferSize);
    }

    public async Task SendAsync(TMessage message, CancellationToken cancellationToken)
    {
        using var memoryOwner = SendingPool.Rent(message.Length);
        var memory = memoryOwner.Memory[.. message.Length];
        message.CopyTo(memory.Span);
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await base.Send(memory, cancellationToken);
    }

    protected override Task ProcessData(ReadOnlyMemory<byte> data, CancellationToken cancellationToken) =>
        messageProcessor.Process(data, cancellationToken);

    /// <summary>
    /// Processes a single received message. This is overridden in derived classes.
    /// </summary>
    /// <param name="message">The message received.</param>
    protected abstract Task ProcessMessage(TMessage message, CancellationToken cancellationToken);
}
