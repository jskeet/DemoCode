using System.Buffers.Binary;
using System.Collections.Immutable;

namespace DigiMixer.Yamaha.Core;

public sealed class YamahaUInt16Segment(ImmutableList<ushort> values) : YamahaSegment
{
    internal override int Length => 5 + Values.Count * 2;

    public ImmutableList<ushort> Values { get; } = values;

    internal override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) YamahaSegmentFormat.UInt16;
        BinaryPrimitives.WriteInt32BigEndian(buffer[1..], Values.Count);
        for (int i = 0; i < Values.Count; i++)
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer[(5 + i * 2)..], Values[i]);
        }
    }

    public static YamahaUInt16Segment Parse(ReadOnlySpan<byte> buffer)
    {
        var valueCount = BinaryPrimitives.ReadInt32BigEndian(buffer[1..]);
        var values = new ushort[valueCount];
        for (int i = 0; i < valueCount; i++)
        {
            values[i] = BinaryPrimitives.ReadUInt16BigEndian(buffer[(5 + i * 2)..]);
        }
        return new YamahaUInt16Segment([.. values]);
    }
}
