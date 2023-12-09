using DigiMixer.Core;

namespace DigiMixer.QuSeries.Core;

/// <summary>
/// A message sent to or from a Qu mixer over TCP.
/// </summary>
public abstract class QuControlMessage
{
    /// <summary>
    /// The total message length on the wire.
    /// </summary>
    public abstract int Length { get; }

    public static QuControlMessage Create(byte type, byte[] data) => new QuGeneralMessage(type, data);

    public static QuControlMessage? TryParse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
        {
            return null;
        }
        // Variable length message
        if (data[0] == 0x7f)
        {
            byte type = data[1];
            int dataLength = LittleEndian.ReadInt16(data.Slice(2));
            if (data.Length < dataLength + 4)
            {
                return null;
            }
            return new QuGeneralMessage(type, data.Slice(4, dataLength).ToArray());
        }
        // Fixed length message
        else if (data[0] == 0xf7)
        {
            if (data.Length < 9)
            {
                return null;
            }
            return new QuValueMessage(data.Slice(1, 8).ToArray());
        }
        else
        {
            return null;
        }
    }

    public abstract byte[] ToByteArray();
}
