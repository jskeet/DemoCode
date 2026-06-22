using System.Buffers.Binary;
using System.Collections.Immutable;

namespace DigiMixer.DmSeries.Core;

public sealed class DmUInt16Segment(ImmutableList<ushort> values) : DmSegment
{
    public override DmSegmentFormat Format => DmSegmentFormat.UInt16;

    public override int Length => 5 + Values.Count * 2;

    public ImmutableList<ushort> Values => values;

    public override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) Format;
        BinaryPrimitives.WriteInt32BigEndian(buffer[1..], Values.Count);
        for (int i = 0; i < Values.Count; i++)
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer[(5 + i * 2)..], Values[i]);
        }
    }

    public static DmUInt16Segment Parse(ReadOnlySpan<byte> buffer)
    {
        var valueCount = BinaryPrimitives.ReadInt32BigEndian(buffer[1..]);
        var values = new ushort[valueCount];
        for (int i = 0; i < valueCount; i++)
        {
            values[i] = BinaryPrimitives.ReadUInt16BigEndian(buffer[(5 + i * 2)..]);
        }
        return new DmUInt16Segment([.. values]);
    }
}
