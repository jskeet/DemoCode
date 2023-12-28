using DigiMixer.Core;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;

namespace DigiMixer.DmSeries.Core;

public class DmMessage : IMixerMessage<DmMessage>
{
    public string Type { get; }

    /// <summary>
    /// Message length including header and data.
    /// </summary>
    public int Length =>
        4 + // Type
        4 + // Message length (excluding type + length)
        5 + // Overall binary container
        4 + // Flags
        Segments.Sum(seg => seg.Length);

    public uint Flags { get; }

    public IReadOnlyList<DmSegment> Segments { get; }

    public DmMessage(string type, uint flags, ImmutableList<DmSegment> segments)
    {
        Type = type;
        Flags = flags;
        Segments = segments;
    }

    public static DmMessage? TryParse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8)
        {
            return null;
        }
        int bodyLength = BinaryPrimitives.ReadInt32BigEndian(data[4..8]);
        if (data.Length < bodyLength + 8)
        {
            return null;
        }
        if (bodyLength < 0)
        {
            throw new InvalidDataException($"Negative body length: {bodyLength}");
        }
        var type = Encoding.ASCII.GetString(data[0..4]).Trim('\0');
        var body = data.Slice(8, bodyLength);
        if (body[0] != (byte) DmSegmentFormat.Binary)
        {
            throw new InvalidDataException("Expected overall container with format 0x11");
        }
        var containerLength = BinaryPrimitives.ReadInt32BigEndian(body.Slice(1));
        if (containerLength != bodyLength - 5)
        {
            throw new InvalidDataException($"Expected overall container internal length {bodyLength - 5}; was {containerLength}");
        }
        var segments = new List<DmSegment>();
        var flags = BinaryPrimitives.ReadUInt32BigEndian(body.Slice(5));
        var nextSegmentData = body.Slice(9);
        while (nextSegmentData.Length > 0)
        {
            var format = (DmSegmentFormat) nextSegmentData[0];
            DmSegment segment = format switch
            {
                DmSegmentFormat.Text => DmTextSegment.Parse(nextSegmentData),
                DmSegmentFormat.Binary => DmBinarySegment.Parse(nextSegmentData),
                DmSegmentFormat.Int32 => DmInt32Segment.Parse(nextSegmentData),
                DmSegmentFormat.UInt32 => DmUInt32Segment.Parse(nextSegmentData),
                DmSegmentFormat.UInt16 => DmUInt16Segment.Parse(nextSegmentData),
                _ => throw new InvalidDataException($"Unexpected segment format {nextSegmentData[0]:x2}")
            };
            segments.Add(segment);
            nextSegmentData = nextSegmentData.Slice(segment.Length);
        }
        return new DmMessage(type, flags, segments.ToImmutableList());
    }

    public void CopyTo(Span<byte> buffer)
    {
        // Note: we assume the span is right-sized to Length.
        Encoding.ASCII.GetBytes(Type, buffer);
        BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(4), buffer.Length - 8);
        buffer[8] = (byte) DmSegmentFormat.Binary;
        BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(9), buffer.Length - 13);
        BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(13), Flags);
        buffer = buffer.Slice(17);
        foreach (var segment in Segments)
        {
            segment.WriteTo(buffer);
            buffer = buffer.Slice(segment.Length);
        }
    }

    public override string ToString() => $"{Type.PadRight(4)}: Flags={Flags:x8}; Segments={Segments.Count}";
}
