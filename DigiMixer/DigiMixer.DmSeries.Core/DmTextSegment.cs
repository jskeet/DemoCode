using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.DmSeries.Core;

public sealed class DmTextSegment(string text) : DmSegment
{
    public override DmSegmentFormat Format => DmSegmentFormat.Text;

    public override int Length => Text.Length + 6;

    /// <summary>
    /// The text of the segment, not including the null terminator.
    /// </summary>
    public string Text => text;

    public override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) Format;
        BinaryPrimitives.WriteInt32BigEndian(buffer[1..], Text.Length + 1);
        Encoding.ASCII.GetBytes(Text, buffer[5..]);
        buffer[5 + Text.Length] = 0;
    }

    public static DmTextSegment Parse(ReadOnlySpan<byte> buffer)
    {
        var textLength = BinaryPrimitives.ReadInt32BigEndian(buffer[1..]);
        // We assume the null termination.
        var text = Encoding.ASCII.GetString(buffer.Slice(5, textLength - 1));
        return new DmTextSegment(text);
    }
}
