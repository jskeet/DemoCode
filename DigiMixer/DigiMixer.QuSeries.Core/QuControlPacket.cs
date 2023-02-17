namespace DigiMixer.QuSeries.Core;

/// <summary>
/// A packet sent to or from a Qu mixer over TCP.
/// </summary>
public abstract class QuControlPacket
{
    /// <summary>
    /// The total packet length on the wire.
    /// </summary>
    public abstract int Length { get; }

    public static QuControlPacket Create(byte type, byte[] data) => new QuGeneralPacket(type, data);

    public static QuControlPacket? TryParse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
        {
            return null;
        }
        // Variable length packet
        if (data[0] == 0x7f)
        {
            byte type = data[1];
            int dataLength = data[2] | data[3] << 8;
            if (data.Length < dataLength + 4)
            {
                return null;
            }
            return new QuGeneralPacket(type, data.Slice(4, dataLength).ToArray());
        }
        // Fixed length packet
        else if (data[0] == 0xf7)
        {
            if (data.Length < 9)
            {
                return null;
            }
            return new QuValuePacket(data.Slice(1, 8).ToArray());
        }
        else
        {
            return null;
        }
    }

    public abstract byte[] ToByteArray();
}
