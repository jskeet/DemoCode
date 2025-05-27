
using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.Yamaha.Core;

public sealed class YamahaTextSegment(string text) : YamahaSegment
{
    /// <summary>
    /// The text of the segment, not including the null terminator.
    /// </summary>
    public string Text { get; } = text;

    internal override int Length => Text.Length + 6;

    internal override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte)YamahaSegmentFormat.Text;
        BinaryPrimitives.WriteInt32BigEndian(buffer[1..], Text.Length + 1);
        Encoding.ASCII.GetBytes(Text, buffer[5..]);
        buffer[5 + Text.Length] = 0;
    }

    public static YamahaTextSegment Parse(ReadOnlySpan<byte> buffer)
    {
        var textLength = BinaryPrimitives.ReadInt32BigEndian(buffer[1..]);
        // We assume the null termination.
        var text = Encoding.ASCII.GetString(buffer.Slice(5, textLength - 1));
        return new YamahaTextSegment(text);
    }
}
