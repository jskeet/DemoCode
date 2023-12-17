using DigiMixer.Core;
using DigiMixer.DmSeries.Core;

namespace DigiMixer.DmSeries.Tools;

internal static class DmMessageExtensions
{
    internal static void DisplaySummary(this DmMessage message, string direction)
    {
        Console.WriteLine($"{direction} {message}: {string.Join(", ", message.Segments.Select(SummarizeSegment))}");

        static string SummarizeSegment(DmSegment segment) => segment switch
        {
            DmBinarySegment => $"Binary[{segment.Length}]",
            DmTextSegment text => $"Text['{text.Text}']",
            DmInt32Segment int32 => $"Int32[*{int32.Values.Count}]",
            DmUInt32Segment uint32 => $"UInt32[*{uint32.Values.Count}]",
            DmUInt16Segment uint16 => $"UInt16[*{uint16.Values.Count}]",
            _ => throw new InvalidOperationException("Unknown segment type")
        };
    }

    internal static void DisplayStructure(this DmMessage message, string direction)
    {
        Console.WriteLine($"{direction} {message}");
        foreach (var segment in message.Segments)
        {
            Console.WriteLine($"  {DescribeSegment(segment)}");
        }
        Console.WriteLine();

        static string DescribeSegment(DmSegment segment)
        {
            switch (segment)
            {
                case DmBinarySegment binary:
                    var data = binary.Data;
                    var hexLength = Math.Min(data.Length, 16);
                    var hex = Formatting.ToHex(data.Slice(0, hexLength)) + (hexLength == data.Length ? "" : " [...]");
                    return $"Binary[{data.Length}]: {hex}";
                case DmTextSegment text:
                    return $"Text: '{text.Text}'";
                case DmInt32Segment int32:
                    return $"Int32[*{int32.Values.Count}]: {string.Join(" ", int32.Values.Select(v => $"0x{v:x8}"))} / {string.Join(" ", int32.Values)}";
                case DmUInt32Segment uint32:
                    return $"UInt32[*{uint32.Values.Count}]: {string.Join(" ", uint32.Values.Select(v => v.ToString("x8")))}";
                case DmUInt16Segment uint16:
                    return $"UInt16[*{uint16.Values.Count}]: {string.Join(" ", uint16.Values.Select(v => v.ToString("x4")))}";
                default:
                    throw new InvalidOperationException("Unknown segment type");
            }
        }
    }
}
