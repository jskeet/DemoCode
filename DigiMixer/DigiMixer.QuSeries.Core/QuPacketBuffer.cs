namespace DigiMixer.QuSeries.Core;

// TODO: Probably extract anything Qu-specific here, and put it somewhere common.
public class QuPacketBuffer
{
    private readonly byte[] buffer;
    private int currentLength;

    public QuPacketBuffer(int bufferSize = 65540)
    {
        buffer = new byte[bufferSize];
    }

    // TODO: Asynchronous action?
    public void Process(ReadOnlySpan<byte> data, Action<QuPacket> action)
    {
        var span = buffer.AsSpan();
        data.CopyTo(span.Slice(currentLength));
        currentLength += data.Length;
        int start = 0;
        while (QuPacket.TryParse(span.Slice(start, currentLength - start)) is QuPacket packet)
        {
            action(packet);
            start += packet.Length;
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
