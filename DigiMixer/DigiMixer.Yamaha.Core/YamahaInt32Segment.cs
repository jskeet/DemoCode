using System.Buffers.Binary;
using System.Collections.Immutable;

namespace DigiMixer.Yamaha.Core;

public sealed class YamahaInt32Segment(ImmutableList<int> values) : YamahaSegment
{
    internal override int Length => 5 + Values.Count * 4;

    public ImmutableList<int> Values { get; } = values;

    internal override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) YamahaSegmentFormat.Int32;
        BinaryPrimitives.WriteInt32BigEndian(buffer[1..], Values.Count);
        for (int i = 0; i < Values.Count; i++)
        {
            BinaryPrimitives.WriteInt32BigEndian(buffer[(5 + i * 4)..], Values[i]);
        }
    }

    public static YamahaInt32Segment Parse(ReadOnlySpan<byte> buffer)
    {
        var valueCount = BinaryPrimitives.ReadInt32BigEndian(buffer[1..]);
        var values = new int[valueCount];
        for (int i = 0; i < valueCount; i++)
        {
            values[i] = BinaryPrimitives.ReadInt32BigEndian(buffer[(5 + i * 4)..]);
        }
        return new YamahaInt32Segment([.. values]);
    }
}
