using DigiMixer.Core;
using DigiMixer.Diagnostics;
using DigiMixer.Yamaha;
using DigiMixer.Yamaha.Core;
using System.Security.Cryptography;

namespace DigiMixer.TfSeries.Tools;

internal static class YamahaMessageExtensions
{
    internal static void DisplayStructure(this AnnotatedMessage<YamahaMessage> annotatedMessage, DecodingOptions options, TextWriter writer, Func<string, SchemaCol?>? schemaProvider = null)
    {
        var message = annotatedMessage.Message;
        if (options.SkipKeepAlive && KeepAliveMessage.IsKeepAlive(message))
        {
            return;
        }
        string directionIndicator = annotatedMessage.Direction == MessageDirection.ClientToMixer ? "=>" : "<=";
        writer.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff}: {directionIndicator} 0x{annotatedMessage.StreamOffset:x8} {message}");
        DisplayBody(message, options, writer, schemaProvider);
    }

    internal static void DisplayStructure(this YamahaMessage message, string directionIndicator, DecodingOptions options, TextWriter writer, Func<string, SchemaCol?>? schemaProvider = null)
    {
        if (options.SkipKeepAlive && KeepAliveMessage.IsKeepAlive(message))
        {
            return;
        }
        writer.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff}: {directionIndicator} {message}");
        DisplayBody(message, options, writer, schemaProvider);
    }

    private static void DisplayBody(YamahaMessage message, DecodingOptions options, TextWriter writer, Func<string, SchemaCol?>? schemaProvider = null)
    {
        var wrappedMessage = WrappedMessage.TryParse(message);

        switch (wrappedMessage)
        {
            case SectionSchemaAndDataMessage section:
                writer.WriteLine($"    SectionSchemaAndData ({section.Data.Name})");
                var hash = MD5.HashData(((YamahaBinarySegment) message.Segments[7]).Data);
                writer.WriteLine($"    MD5: {Formatting.ToHex(hash)}");
                if (options.DecodeSchema)
                {
                    DescribeSchema(section.Data);
                }
                if (options.DecodeData)
                {
                    DescribeData(section.Data);
                }
                break;
            case SyncHashesMessage shm:
                writer.WriteLine($"    SyncHashes: {shm.Subtype}");
                break;
            case KeepAliveMessage kam:
                writer.WriteLine("    KeepAlive");
                break;
            case SingleValueMessage svm:
                writer.WriteLine($"    SingleValue ({svm.SectionName}): Value={DescribeSegment(svm.ValueSegment)}");
                if (schemaProvider?.Invoke(svm.SectionName) is SchemaCol schema)
                {
                    var property = svm.ResolveProperty(schema);
                    writer.WriteLine($"    Property: {property.Path} (Indexes {string.Join(", ", svm.SchemaIndexes)})");
                }
                break;
        }
        if (wrappedMessage is null || options.ShowAllSegments)
        {
            foreach (var segment in message.Segments)
            {
                writer.WriteLine($"  {DescribeSegment(segment)}");
            }
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

        void DescribeSchema(SectionSchemaAndData section)
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
                writer.WriteLine($"{nestedIndent}PR: {property.Name}; Type={property.Type}; Length={property.Length}; Count={property.Count}");
            }
            foreach (var nested in col.Cols)
            {
                DescribeCol(nested, nestedIndent);
            }
        }

        void DescribeData(SectionSchemaAndData section)
        {
            DescribeColData(section, section.Schema, "", "      ", 0);
        }

        void DescribeColData(SectionSchemaAndData section, SchemaCol col, string colIndex, string indent, int additionalOffset)
        {
            writer.WriteLine($"{indent}COL{colIndex}: {col.Name}");
            foreach (var property in col.Properties)
            {
                for (int i = 0; i < property.Count; i++)
                {
                    int propertyAdditionalOffset = additionalOffset + i * property.Length;
                    string description = property.Type switch
                    {
                        SchemaPropertyType.Text => section.GetString(property, propertyAdditionalOffset),
                        SchemaPropertyType.UnsignedInteger => property.Length switch
                        {
                            1 => section.GetUInt8(property, propertyAdditionalOffset).ToString(),
                            2 => section.GetUInt16(property, propertyAdditionalOffset).ToString(),
                            4 => section.GetUInt32(property, propertyAdditionalOffset).ToString(),
                            _ => throw new InvalidOperationException($"Unexpected length {property.Length}")
                        },
                        SchemaPropertyType.SignedInteger => property.Length switch
                        {
                            1 => section.GetInt8(property, propertyAdditionalOffset).ToString(),
                            2 => section.GetInt16(property, propertyAdditionalOffset).ToString(),
                            4 => section.GetInt32(property, propertyAdditionalOffset).ToString(),
                            _ => throw new InvalidOperationException($"Unexpected length {property.Length}")
                        },
                        _ => throw new InvalidOperationException($"Unexpected property type {property.Type}")
                    };
                    string index = property.Count == 1 ? "" : $"[{i}]";
                    writer.WriteLine($"{indent}  {property.Name}{index}: {description}");
                }
            }
            foreach (var nestedCol in col.Cols)
            {
                for (int i = 0; i < nestedCol.Count; i++)
                {
                    string nestedColIndex = nestedCol.Count == 1 ? "" : $"[{i}]";
                    DescribeColData(section, nestedCol, nestedColIndex, indent + "  ", additionalOffset + i * nestedCol.DataLength);
                }
            }
        }
    }
}
