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

    public override string ToString()
    {
        var hexSpan = Data.Length <= 10 ? Data : Data.Slice(0, 10);
        string dataDescription = BitConverter.ToString(hexSpan.ToArray()) + (Data.Length <= 10 ? "" : $"... ({Data.Length} bytes)");
        return $"Received: {Type}: {dataDescription}";
    }
}
