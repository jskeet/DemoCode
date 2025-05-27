using System.Buffers.Binary;

namespace DigiMixer.Yamaha.Core;

public sealed class YamahaBinarySegment : YamahaSegment
{
    public static YamahaBinarySegment Empty { get; } = new([]);
    public static YamahaBinarySegment Zero16 { get; } = new(new byte[16]);

    internal override int Length => data.Length + 5;

    private readonly ReadOnlyMemory<byte> data;
    public ReadOnlySpan<byte> Data => data.Span;

    private YamahaBinarySegment(byte[] data)
    {
        this.data = data;
    }

    public YamahaBinarySegment(ReadOnlySpan<byte> data) : this(data.ToArray())
    {
    }

    public static YamahaBinarySegment FromHex(string text)
    {
        text = text.Replace(" ", "");
        byte[] data = new byte[text.Length / 2];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte) ((ParseNybble(text[i * 2]) << 4) + ParseNybble(text[i * 2 + 1]));
        }
        return new YamahaBinarySegment(data);

        static int ParseNybble(char c) => c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
            >= 'a' and <= 'f' => c - 'a' + 10,
            _ => throw new ArgumentException($"Invalid nybble '{c}'")
        };
    }

    internal override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) YamahaSegmentFormat.Binary;
        BinaryPrimitives.WriteInt32BigEndian(buffer[1..], data.Length);
        Data.CopyTo(buffer[5..]);
    }

    public static YamahaBinarySegment Parse(ReadOnlySpan<byte> buffer)
    {
        var dataLength = BinaryPrimitives.ReadInt32BigEndian(buffer[1..]);
        var bytes = new byte[dataLength];
        buffer.Slice(5, dataLength).CopyTo(bytes);
        return new YamahaBinarySegment(bytes);
    }
}
