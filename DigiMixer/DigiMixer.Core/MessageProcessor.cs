namespace DigiMixer.Core;

/// <summary>
/// Message processor with internal buffer, used to handle incoming TCP streams.
/// </summary>
public sealed class MessageProcessor<TMessage> where TMessage : class, IMixerMessage<TMessage>
{
    private readonly Func<TMessage, CancellationToken, Task> messageAction;
    private readonly Memory<byte> buffer;

    /// <summary>
    /// The amount of unprocessed data left in the buffer.
    /// </summary>
    public int UnprocessedLength { get; private set; }

    /// <summary>
    /// The total number of messages processed.
    /// </summary>
    public long MessagesProcessed { get; private set; }

    public MessageProcessor(Func<TMessage, CancellationToken, Task> messageAction, int bufferSize = 65540)
    {
        this.messageAction = messageAction;
        buffer = new byte[bufferSize];
    }

    public MessageProcessor(Action<TMessage> messageAction, int bufferSize = 65540)
        : this((message, cancellationToken) => { messageAction(message); return Task.CompletedTask; }, bufferSize)
    {
    }

    /// <summary>
    /// Asynchronously processes the data from <paramref name="data"/>, retaining any data
    /// that isn't part of a message. The data may contain multiple messages, and each will be
    /// processed separately.
    /// </summary>
    public async Task Process(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        data.CopyTo(buffer.Slice(UnprocessedLength));
        UnprocessedLength += data.Length;
        int start = 0;
        while (TMessage.TryParse(buffer.Slice(start, UnprocessedLength - start).Span) is TMessage message)
        {
            MessagesProcessed++;
            await messageAction(message, cancellationToken);
            start += message.Length;
        }
        // If we've consumed the whole buffer, reset to the start. (No copying required.)
        if (start == UnprocessedLength)
        {
            UnprocessedLength = 0;
        }
        // Otherwise, copy whatever's left.
        else
        {
            buffer.Slice(start, UnprocessedLength - start).CopyTo(buffer);
            UnprocessedLength -= start;
        }
    }
}
