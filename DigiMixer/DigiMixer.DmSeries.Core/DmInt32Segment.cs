using System.Buffers.Binary;
using System.Collections.Immutable;

namespace DigiMixer.DmSeries.Core;

public sealed class DmInt32Segment : DmSegment
{
    public override DmSegmentFormat Format => DmSegmentFormat.Int32;

    public override int Length => 5 + Values.Count * 4;

    public ImmutableList<int> Values { get; }

    public DmInt32Segment(ImmutableList<int> values)
    {
        Values = values;
    }

    public override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) Format;
        BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(1), Values.Count);
        for (int i = 0; i < Values.Count; i++)
        {
            BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(5 + i * 4), Values[i]);
        }
    }

    public static DmInt32Segment Parse(ReadOnlySpan<byte> buffer)
    {
        var valueCount = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(1));
        var values = new int[valueCount];
        for (int i = 0; i < valueCount; i++)
        {
            values[i] = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(5 + i * 4));
        }
        return new DmInt32Segment(values.ToImmutableList());
    }
}
