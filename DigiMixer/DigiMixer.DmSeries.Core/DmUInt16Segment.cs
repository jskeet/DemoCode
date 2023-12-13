using DigiMixer.Core;
using System.Collections.Immutable;

namespace DigiMixer.DmSeries.Core;

public sealed class DmUInt16Segment : DmSegment
{
    public override DmSegmentFormat Format => DmSegmentFormat.Int32;

    public override int Length => 5 + Values.Count * 2;

    public ImmutableList<ushort> Values { get; }

    public DmUInt16Segment(ImmutableList<ushort> values)
    {
        Values = values;
    }

    public override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) Format;
        BigEndian.WriteInt32(buffer.Slice(1), Values.Count);
        for (int i = 0; i < Values.Count; i++)
        {
            BigEndian.WriteUInt16(buffer.Slice(5 + i * 2), Values[i]);
        }
    }

    public static DmUInt16Segment Parse(ReadOnlySpan<byte> buffer)
    {
        var valueCount = BigEndian.ReadInt32(buffer.Slice(1));
        var values = new ushort[valueCount];
        for (int i = 0; i < valueCount; i++)
        {
            values[i] = BigEndian.ReadUInt16(buffer.Slice(5 + i * 2));
        }
        return new DmUInt16Segment(values.ToImmutableList());
    }
}
