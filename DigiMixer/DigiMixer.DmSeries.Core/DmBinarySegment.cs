using System.Buffers.Binary;

namespace DigiMixer.DmSeries.Core;

public sealed class DmBinarySegment : DmSegment
{
    public override DmSegmentFormat Format => DmSegmentFormat.Binary;

    public override int Length => data.Length + 5;

    private readonly ReadOnlyMemory<byte> data;
    public ReadOnlySpan<byte> Data => data.Span;

    private DmBinarySegment(byte[] data)
    {
        this.data = data;
    }

    public DmBinarySegment(ReadOnlySpan<byte> data) : this(data.ToArray())
    {
    }

    public static DmBinarySegment FromHex(string text)
    {
        text = text.Replace(" ", "");
        byte[] data = new byte[text.Length / 2];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte) ((ParseNybble(text[i * 2]) << 4) + ParseNybble(text[i * 2 + 1]));
        }
        return new DmBinarySegment(data);

        static int ParseNybble(char c) => c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
            >= 'a' and <= 'f' => c - 'a' + 10,
            _ => throw new ArgumentException($"Invalid nybble '{c}'")
        };
    }

    public override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) Format;
        BinaryPrimitives.WriteInt32BigEndian(buffer[1..], data.Length);
        Data.CopyTo(buffer[5..]);
    }

    public static DmBinarySegment Parse(ReadOnlySpan<byte> buffer)
    {
        var dataLength = BinaryPrimitives.ReadInt32BigEndian(buffer[1..]);
        var bytes = new byte[dataLength];
        buffer.Slice(5, dataLength).CopyTo(bytes);
        return new DmBinarySegment(bytes);
    }
}
