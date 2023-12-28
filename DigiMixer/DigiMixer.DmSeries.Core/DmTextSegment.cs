using DigiMixer.Core;
using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.DmSeries.Core;

public sealed class DmTextSegment : DmSegment
{
    public override DmSegmentFormat Format => DmSegmentFormat.Text;

    public override int Length => Text.Length + 6;

    /// <summary>
    /// The text of the segment, not including the null terminator.
    /// </summary>
    public string Text { get; }

    public DmTextSegment(string text)
    {
        Text = text;
    }

    public override void WriteTo(Span<byte> buffer)
    {
        buffer[0] = (byte) Format;
        BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(1), Text.Length + 1);
        Encoding.ASCII.GetBytes(Text, buffer.Slice(5));
        buffer[5 + Text.Length] = 0;
    }

    public static DmTextSegment Parse(ReadOnlySpan<byte> buffer)
    {
        var textLength = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(1));
        // We assume the null termination.
        var text = Encoding.ASCII.GetString(buffer.Slice(5, textLength - 1));
        return new DmTextSegment(text);
    }
}
