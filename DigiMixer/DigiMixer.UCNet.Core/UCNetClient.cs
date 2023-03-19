using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.UCNet.Core;

public class UCNetClient : TcpControllerBase
{
    public event EventHandler<UCNetMessage>? MessageReceived;

    // TODO: Reuse something like QuPacketBuffer
    private int position;
    private byte[] buffer;

    public UCNetClient(ILogger logger, string host, int port) : base(logger, host, port)
    {
        buffer = new byte[65542];
    }

    public async Task Send(UCNetMessage message, CancellationToken cancellationToken)
    {
        var data = message.ToByteArray();
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await Send(data, cancellationToken);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data)
    {
        // The index of the start of the next message. It moves through the buffer as we manage to parse.
        // Before reading data, it's always at the start of the buffer.
        int start = 0;
        var bytesRead = data.Length;
        data.CopyTo(buffer.AsSpan().Slice(position));
        // The index at the end of where we have valid data.
        int end = position + bytesRead;
        while (UCNetMessage.TryParse(buffer.AsSpan()[start..end]) is UCNetMessage message)
        {
            MessageReceived?.Invoke(this, message);
            start += message.Length;
        }
        // If we've consumed the whole buffer, reset to the start. (No copying required.)
        if (start == end)
        {
            position = 0;
        }
        // Otherwise, copy whatever's left.
        else
        {
            Buffer.BlockCopy(buffer, start, buffer, 0, end - start);
            position = end - start;
        }
    }
}
