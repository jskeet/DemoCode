using System.Buffers.Binary;
using System.Collections.Immutable;

namespace DigiMixer.Yamaha.Core;

public sealed class YamahaUInt32Segment(ImmutableList<uint> values) : YamahaSegment
{
    internal override int Length => 5 + Values.Count * 4;

    public ImmutableList<uint> Values { get; } = values;

    internal override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) YamahaSegmentFormat.UInt32;
        BinaryPrimitives.WriteInt32BigEndian(buffer[1..], Values.Count);
        for (int i = 0; i < Values.Count; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(buffer[(5 + i * 4)..], Values[i]);
        }
    }

    public static YamahaUInt32Segment Parse(ReadOnlySpan<byte> buffer)
    {
        var valueCount = BinaryPrimitives.ReadInt32BigEndian(buffer[1..]);
        var values = new uint[valueCount];
        for (int i = 0; i < valueCount; i++)
        {
            values[i] = BinaryPrimitives.ReadUInt32BigEndian(buffer[(5 + i * 4)..]);
        }
        return new YamahaUInt32Segment([.. values]);
    }
}
