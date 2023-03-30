namespace DigiMixer.Core;

/// <summary>
/// Message processor with internal buffer, used to handle incoming TCP streams.
/// </summary>
public sealed class MessageProcessor<TMessage> where TMessage : class
{
    public delegate TMessage? Parser(ReadOnlySpan<byte> data);

    private readonly Parser messageParser;
    private readonly Func<TMessage, int> messageLengthExtractor;
    private readonly Action<TMessage> messageAction;
    private readonly byte[] buffer;
    private int currentLength;

    public MessageProcessor(Parser messageParser, Func<TMessage, int> messageLengthExtractor, Action<TMessage> messageAction, int bufferSize = 65540)
    {
        this.messageParser = messageParser;
        this.messageLengthExtractor = messageLengthExtractor;
        this.messageAction = messageAction;
        buffer = new byte[bufferSize];
    }

    /// <summary>
    /// Synchronously processes the data from <paramref name="data"/>, retaining any data
    /// that isn't part of a message. The data may contain multiple messages, and each will be
    /// processed separately.
    /// </summary>
    /// <remarks>
    /// This is currently synchronous, which seems to be "okay"; we could potentially change it to be asynchronous
    /// later.
    /// </remarks>
    public void Process(ReadOnlySpan<byte> data)
    {
        var span = buffer.AsSpan();
        data.CopyTo(span.Slice(currentLength));
        currentLength += data.Length;
        int start = 0;
        while (messageParser(span.Slice(start, currentLength - start)) is TMessage message)
        {
            messageAction(message);
            start += messageLengthExtractor(message);
        }
        // If we've consumed the whole buffer, reset to the start. (No copying required.)
        if (start == currentLength)
        {
            currentLength = 0;
        }
        // Otherwise, copy whatever's left.
        else
        {
            Buffer.BlockCopy(buffer, start, buffer, 0, currentLength - start);
            currentLength -= start;
        }
    }
}
