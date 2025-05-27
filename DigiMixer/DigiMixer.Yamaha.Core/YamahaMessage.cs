using DigiMixer.Core;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;

namespace DigiMixer.Yamaha.Core;

public class YamahaMessage : IMixerMessage<YamahaMessage>
{
    /// <summary>
    /// Message type, e.g. MPRO or EEVT.
    /// </summary>
    public YamahaMessageType Type { get; }

    /// <summary>
    /// The 32-bit value between the message length and the segments.
    /// </summary>
    public uint Header { get; }

    public int Length =>
        4 // Type
        + 4 // Message length (excluding type and length)
        + 1 // Binary segment
        + 4 // Length of binary segment
        + 4 // Header
        + Segments.Sum(s => s.Length); // Nested segments

    public ImmutableList<YamahaSegment> Segments { get; }

    public YamahaMessage(YamahaMessageType type, uint header, ImmutableList<YamahaSegment> segments)
    {
        Type = type;
        Header = header;
        Segments = segments;
        if ((Header & 0xff) != segments.Count)
        {
            throw new ArgumentException($"Header {Header:x8} incompatible with segment count of {Segments.Count}");
        }
        if (((Header >> 24) & 0xff) != type.HeaderByte)
        {
            throw new ArgumentException($"Header {Header:x8} incompatible with  message type {Type}");
        }
    }

    public static YamahaMessage? TryParse(ReadOnlySpan<byte> data)
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
        if (YamahaMessageType.TryParse(data) is not YamahaMessageType type)
        {
            throw new InvalidDataException($"Unknown message type: {Encoding.ASCII.GetString(data[0..4]).Trim()}");
        }
        var body = data.Slice(8, bodyLength);
        if (body[0] != (byte) YamahaSegmentFormat.Binary)
        {
            throw new InvalidDataException("Expected overall container with format 0x11");
        }
        var containerLength = BinaryPrimitives.ReadInt32BigEndian(body[1..]);
        if (containerLength != bodyLength - 5)
        {
            throw new InvalidDataException($"Expected overall container internal length {bodyLength - 5}; was {containerLength}");
        }
        var segments = new List<YamahaSegment>();
        var header = BinaryPrimitives.ReadUInt32BigEndian(body[5..]);
        var nextSegmentData = body[9..];
        while (nextSegmentData.Length > 0)
        {
            var format = (YamahaSegmentFormat)nextSegmentData[0];
            YamahaSegment segment = format switch
            {
                YamahaSegmentFormat.Text => YamahaTextSegment.Parse(nextSegmentData),
                YamahaSegmentFormat.Binary => YamahaBinarySegment.Parse(nextSegmentData),
                YamahaSegmentFormat.Int32 => YamahaInt32Segment.Parse(nextSegmentData),
                YamahaSegmentFormat.UInt32 => YamahaUInt32Segment.Parse(nextSegmentData),
                YamahaSegmentFormat.UInt16 => YamahaUInt16Segment.Parse(nextSegmentData),
                _ => throw new InvalidDataException($"Unexpected segment format {nextSegmentData[0]:x2}")
            };
            segments.Add(segment);
            nextSegmentData = nextSegmentData[segment.Length..];
        }
        return new YamahaMessage(type, header, [.. segments]);
    }

    public void CopyTo(Span<byte> buffer)
    {
        // Note: we assume the span is right-sized to Length.
        Type.WriteTo(buffer);
        BinaryPrimitives.WriteInt32BigEndian(buffer[4..], buffer.Length - 8);
        buffer[8] = (byte) YamahaSegmentFormat.Binary;
        BinaryPrimitives.WriteInt32BigEndian(buffer[9..], buffer.Length - 13);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[13..], Header);
        buffer = buffer[17..];
        foreach (var segment in Segments)
        {
            segment.WriteTo(buffer);
            buffer = buffer[segment.Length..];
        }
    }

    public override string ToString() => $"{Type.Text,-4}: Flag1={(Header>>16) & 0xff:x2}; Flag2={(Header >> 8) & 0xff:x2}; Segments={Segments.Count}";
}
