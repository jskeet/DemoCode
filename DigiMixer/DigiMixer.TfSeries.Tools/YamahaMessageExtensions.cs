using DigiMixer.Core;
using DigiMixer.Diagnostics;
using DigiMixer.Yamaha;
using DigiMixer.Yamaha.Core;
using System.Buffers.Binary;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace DigiMixer.TfSeries.Tools;

internal static class YamahaMessageExtensions
{
    internal static void DisplaySummary(this YamahaMessage message, string direction)
    {
        Console.WriteLine($"{direction} {message}: {string.Join(", ", message.Segments.Select(SummarizeSegment))}");

        static string SummarizeSegment(YamahaSegment segment) => segment switch
        {
            YamahaBinarySegment binary => $"Binary[{binary.Data.Length}]",
            YamahaTextSegment text => $"Text['{text.Text}']",
            YamahaInt32Segment int32 => $"Int32[*{int32.Values.Count}]",
            YamahaUInt32Segment uint32 => $"UInt32[*{uint32.Values.Count}]",
            YamahaUInt16Segment uint16 => $"UInt16[*{uint16.Values.Count}]",
            _ => throw new InvalidOperationException("Unknown segment type")
        };
    }

    internal static void DisplayStructure(this AnnotatedMessage<YamahaMessage> annotatedMessage, DecodingOptions options, TextWriter writer)
    {
        var message = annotatedMessage.Message;
        if (options.SkipKeepAlive && message.Type.Text == "EEVT" && message.Segments.Count > 3 && message.Segments[0] is YamahaTextSegment { Text: "KeepAlive" })
        {
            return;
        }
        string directionIndicator = annotatedMessage.Direction == MessageDirection.ClientToMixer ? "=>" : "<=";
        writer.WriteLine($"{directionIndicator} 0x{annotatedMessage.StreamOffset:x8} {message}");
        DisplaySegments(message, options, writer);
    }

    internal static void DisplayStructure(this YamahaMessage message, string directionIndicator, DecodingOptions options, TextWriter writer)
    {
        if (options.SkipKeepAlive && message.Type.Text == "EEVT" && message.Segments.Count > 3 && message.Segments[0] is YamahaTextSegment { Text: "KeepAlive" })
        {
            return;
        }
        writer.WriteLine($"{directionIndicator} {message}");
        DisplaySegments(message, options, writer);
    }

    private static void DisplaySegments(YamahaMessage message, DecodingOptions options, TextWriter writer)
    {
        foreach (var segment in message.Segments)
        {
            writer.WriteLine($"  {DescribeSegment(segment)}");
        }
        if (SectionSchemaAndDataMessage.TryParse(message) is { } section && options.DecodeSchemaAndData)
        {
            var hash = MD5.HashData(((YamahaBinarySegment) message.Segments[7]).Data);
            writer.WriteLine($"    MD5: {Formatting.ToHex(hash)}");
            DescribeSchemaAndData(section.Data);
        }
        writer.WriteLine();

        static string DescribeSegment(YamahaSegment segment)
        {
            switch (segment)
            {
                case YamahaBinarySegment binary:
                    var data = binary.Data;
                    var hexLength = Math.Min(data.Length, 16);
                    var hex = Formatting.ToHex(data[..hexLength]) + (hexLength == data.Length ? "" : " [...]");
                    return $"Binary[{data.Length}]: {hex}";
                case YamahaTextSegment text:
                    return $"Text: '{text.Text}'";
                case YamahaInt32Segment int32:
                    return $"Int32[*{int32.Values.Count}]: {string.Join(" ", int32.Values.Select(v => $"0x{v:x8}"))} / {string.Join(" ", int32.Values)}";
                case YamahaUInt32Segment uint32:
                    return $"UInt32[*{uint32.Values.Count}]: {string.Join(" ", uint32.Values.Select(v => v.ToString("x8")))}";
                case YamahaUInt16Segment uint16:
                    return $"UInt16[*{uint16.Values.Count}]: {string.Join(" ", uint16.Values.Select(v => v.ToString("x4")))}";
                default:
                    throw new InvalidOperationException("Unknown segment type");
            }
        }

        void DescribeSchemaAndData(SectionSchemaAndData section)
        {
            writer.WriteLine($"    Hash text: {section.SchemaHash}");
            writer.WriteLine($"    Schema:");
            DescribeCol(section.Schema, "      ");
        }

        void DescribeCol(SchemaCol col, string indent)
        {
            writer.WriteLine($"{indent}COL: {col.Name} Offset={col.RelativeOffset}; Data length={col.DataLength}; Count={col.Count}");
            var nestedIndent = indent + "  ";
            foreach (var property in col.Properties)
            {
                writer.WriteLine($"{nestedIndent}  PR: {property.Name}; Type={property.Type}; Length={property.Length}; Count={property.Count}");
            }
            foreach (var nested in col.Cols)
            {
                DescribeCol(nested, nestedIndent);
            }
        }
    }
}
