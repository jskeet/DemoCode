using DigiMixer.Core;
using System.Buffers.Binary;
using System.Collections.Immutable;

namespace DigiMixer.DmSeries.Core;

public sealed class DmUInt32Segment : DmSegment
{
    public override DmSegmentFormat Format => DmSegmentFormat.UInt32;

    public override int Length => 5 + Values.Count * 4;

    public ImmutableList<uint> Values { get; }

    public DmUInt32Segment(ImmutableList<uint> values)
    {
        Values = values;
    }

    public override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) Format;
        BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(1), Values.Count);
        for (int i = 0; i < Values.Count; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(5 + i * 4), Values[i]);
        }
    }

    public static DmUInt32Segment Parse(ReadOnlySpan<byte> buffer)
    {
        var valueCount = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(1));
        var values = new uint[valueCount];
        for (int i = 0; i < valueCount; i++)
        {
            values[i] = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(5 + i * 4));
        }
        return new DmUInt32Segment(values.ToImmutableList());
    }
}
