namespace DigiMixer.BehringerWing.Core;

/// <summary>
/// Handles buffering, escaping and unescaping for Wing tokens, keeping track of the currently
/// active send/receive channels.
/// This is separated from <see cref="WingClient"/> so that it can easily be unit tested.
/// </summary>
internal class WingTokenProcessor
{
    internal event EventHandler<(WingProtocolChannel, WingToken)>? TokenReceived;

    private WingProtocolChannel writeChannel = WingProtocolChannel.None;
    private WingProtocolChannel readChannel = WingProtocolChannel.None;

    /// <summary>
    /// This is used transiently (during <see cref="WriteTokens(WingProtocolChannel, IEnumerable{WingToken}, Span{byte})"/>)
    /// to store the non-escaped data, which is then either escaped into the caller-passed buffer.
    /// (This means we assume there are no concurrent write calls, but that's okay.)
    /// </summary>
    private readonly Memory<byte> writeBuffer = new byte[32768];

    /// <summary>
    /// This is used to store unescaped data for tokens. The data is unescaped in <see cref="Process"/>
    /// and parsed as far as possible, but some data may be left in the buffer between calls. This is always
    /// stored at the start of the buffer, and the amount of unprocessed data is stored in <see cref="unprocessedReadLength"/>.
    /// </summary>
    private readonly Memory<byte> readBuffer = new byte[90000];
    private int unprocessedReadLength;
    /// <summary>
    /// If the buffer passed to <see cref="Process"/> finishes with an escape byte, we don't know what that will end up
    /// meaning. (It could be a channel change, it could be the first half of an escape pair, it could just be itself.)
    /// This flag remembers that we effectively have to start with the escape byte on the next call. (It's also used
    /// within the method to remember whether we're within an escape sequence.)
    /// </summary>
    private bool midEscapeReading;

    /// <summary>
    /// Writes the token into the given span, escaping if necessary.
    /// All tokens are written to the same channel.
    /// </summary>
    /// <returns>The number of bytes written.</returns>
    internal int WriteTokens(WingProtocolChannel channel, IEnumerable<WingToken> tokens, Span<byte> span)
    {
        int escapedLength = 0;
        if (channel != writeChannel)
        {
            span[0] = 0xdf;
            span[1] = (byte) (0xd0 + (byte) channel);
            writeChannel = channel;
            escapedLength = 2;
        }

        // Write the unescaped token data to our internal buffer.
        var nonEscapedSpan = writeBuffer.Span;
        int nonEscapedLength = 0;
        foreach (var token in tokens)
        {
            nonEscapedLength += token.CopyTo(nonEscapedSpan[nonEscapedLength..]);
        }

        // Now copy it to the caller's buffer, escaping as we go.
        for (int i = 0; i < nonEscapedLength; i++)
        {
            span[escapedLength++] = nonEscapedSpan[i];
            if (nonEscapedSpan[i] == WingConstants.Escape &&
                (i == nonEscapedLength - 1 || (nonEscapedSpan[i + 1] >= WingConstants.MinValueToEscape && nonEscapedSpan[i + 1] <= WingConstants.MaxValueToEscape)))
            {
                span[escapedLength++] = WingConstants.EscapedEscape;
            }
        }
        return escapedLength;
    }

    internal void Process(ReadOnlySpan<byte> data)
    {
        foreach (var db in data)
        {
            if (midEscapeReading)
            {
                midEscapeReading = false;
                // Channel change: only valid between tokens.
                if (db >= WingConstants.MinProtocolChannelChange && db <= WingConstants.MaxProtocolChannelChange)
                {
                    // Process any pending tokens.
                    ProcessReadBuffer();
                    if (unprocessedReadLength != 0)
                    {
                        throw new InvalidDataException("Channel change mid-token");
                    }
                    readChannel = (WingProtocolChannel) (db - WingConstants.MinProtocolChannelChange);
                }
                // Escaped escape: write the escape byte, then continue
                else if (db == WingConstants.EscapedEscape)
                {
                    readBuffer.Span[unprocessedReadLength++] = WingConstants.Escape;
                }
                // Just an escape byte followed by a normal byte: write them both out.
                else
                {
                    readBuffer.Span[unprocessedReadLength++] = WingConstants.Escape;
                    readBuffer.Span[unprocessedReadLength++] = db;
                }
                continue;
            }
            if (db == WingConstants.Escape)
            {
                midEscapeReading = true;
                continue;
            }
            readBuffer.Span[unprocessedReadLength++] = db;
        }
        // Process any tokens we've now unescaped.
        ProcessReadBuffer();
    }

    /// <summary>
    /// Processes the unescaped read buffer as far as possible, moving any remaining data to the start and setting
    /// <see cref="unprocessedReadLength"/> if necessary.
    /// </summary>
    private void ProcessReadBuffer()
    {
        if (unprocessedReadLength == 0)
        {
            return;
        }
        int start = 0;
        while (start < unprocessedReadLength)
        {
            var (token, tokenLength) = WingToken.TryParse(readBuffer.Span[start..unprocessedReadLength]);
            if (token is null)
            {
                break;
            }
            TokenReceived?.Invoke(this, (readChannel, token));
            start += tokenLength;
        }
        // If we've consumed the whole buffer, reset to the start. (No copying required.)
        if (start == unprocessedReadLength)
        {
            unprocessedReadLength = 0;
        }
        // Otherwise, copy whatever's left.
        else
        {
            readBuffer[start..unprocessedReadLength].CopyTo(readBuffer);
            unprocessedReadLength -= start;
        }
    }
}
