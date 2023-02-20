namespace DigiMixer.QuSeries.Core;

/// <summary>
/// A general-purpose packet from/to a Qu mixer, with a type and arbitrary-length data.
/// </summary>
public class QuGeneralPacket : QuControlPacket
{
    public byte Type { get; }
    private readonly byte[] data;
    public ReadOnlySpan<byte> Data => data;

    public override int Length => data.Length + 4;

    internal QuGeneralPacket(byte type, byte[] data)
    {
        Type = type;
        this.data = data;
    }

    public override byte[] ToByteArray()
    {
        var packet = new byte[Length];
        packet[0] = 0x7f;
        packet[1] = Type;
        packet[2] = (byte) (data.Length & 0xff);
        packet[3] = (byte) (data.Length >> 8);
        data.CopyTo(packet.AsSpan().Slice(4));
        return packet;
    }

    private const int FullDataLength = 10;
    public override string ToString()
    {
        var hexSpan = Data.Length <= FullDataLength ? Data : Data.Slice(0, FullDataLength);
        string dataDescription = BitConverter.ToString(hexSpan.ToArray()) + (Data.Length <= FullDataLength ? "" : $"... ({Data.Length} bytes)");
        return $"General: {Type}: {dataDescription}";
    }

    internal bool HasNonZeroData()
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] != 0)
            {
                return true;
            }
        }
        return false;
    }
}
