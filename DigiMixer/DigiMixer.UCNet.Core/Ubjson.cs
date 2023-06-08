using Newtonsoft.Json;
using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.UCNet.Core;

/// <summary>
/// Minimal support for UBJSON (https://ubjson.org/)
/// </summary>
internal static class Ubjson
{
    internal static string ToJson(ReadOnlySpan<byte> data)
    {
        // Note: this uses the real stack for containers, which isn't a brilliant idea really, but it makes the coding simpler.

        int index = 0;

        var writer = new StringWriter();
        var jsonWriter = new JsonTextWriter(writer);

        if (data[0] != '{')
        {
            throw new ArgumentException("Expected to read a JSON object");
        }
        jsonWriter.WriteStartObject();
        index++;
        ParseObject(data);
        if (index != data.Length)
        {
            throw new ArgumentException("Top-level object ended before end of data");
        }
        return writer.ToString();

        void ParseObject(ReadOnlySpan<byte> data)
        {
            while (true)
            {
                if (data[index] == '}')
                {
                    jsonWriter.WriteEndObject();
                    index++;
                    return;
                }
                (var propertyName, index) = ReadString(data, index);
                jsonWriter.WritePropertyName(propertyName);
                switch ((char) data[index])
                {
                    case '{':
                        jsonWriter.WriteStartObject();
                        index++;
                        ParseObject(data);
                        break;
                    case '[':
                        jsonWriter.WriteStartArray();
                        index++;
                        ParseArray(data);
                        break;
                    default:
                        (var value, index) = ReadValue(data, index);
                        WriteValue(value);
                        break;
                }
            }
        }

        void ParseArray(ReadOnlySpan<byte> data)
        {
            while (true)
            {
                switch ((char) data[index])
                {
                    case '{':
                        jsonWriter.WriteStartObject();
                        index++;
                        ParseObject(data);
                        break;
                    case '[':
                        jsonWriter.WriteStartArray();
                        index++;
                        ParseArray(data);
                        break;
                    case ']':
                        jsonWriter.WriteEndArray();
                        index++;
                        return;
                    default:
                        (var value, index) = ReadValue(data, index);
                        WriteValue(value);
                        break;
                }
            }
        }

        void WriteValue(object? value)
        {
            switch (value)
            {
                case null:
                    jsonWriter.WriteNull();
                    break;
                case bool b:
                    jsonWriter.WriteValue(b);
                    break;
                case char c:
                    jsonWriter.WriteValue(c);
                    break;
                case string text:
                    jsonWriter.WriteValue(text);
                    break;
                case int number:
                    jsonWriter.WriteValue(number);
                    break;
                case long number:
                    jsonWriter.WriteValue(number);
                    break;
                case float number:
                    jsonWriter.WriteValue(number);
                    break;
                case double number:
                    jsonWriter.WriteValue(number);
                    break;
                default:
                    throw new InvalidOperationException($"Don't know how to write a value of type {value.GetType()}");
            }
        }

        (object?, int) ReadValue(ReadOnlySpan<byte> data, int start) =>
            (char) data[start] switch
            {
                'S' => ReadString(data, start + 1),
                'Z' => (null, start + 1),
                'T' => (true, start + 1),
                'F' => (false, start + 1),
                // int8, uint8, int16, int32 are all returned as int for convenience (of what we need here)
                'i' => ((int) (sbyte) data[start + 1], start + 2),
                'U' => (data[start + 1], start + 2),
                'I' => (BinaryPrimitives.ReadInt16BigEndian(data.Slice(start + 1, 2)), start + 3),
                'l' => (BinaryPrimitives.ReadInt32BigEndian(data.Slice(start + 1, 4)), start + 5),
                'L' => (BinaryPrimitives.ReadInt64BigEndian(data.Slice(start + 1, 8)), start + 9),
                'd' => (BinaryPrimitives.ReadSingleBigEndian(data.Slice(start + 1, 4)), start + 5),
                'D' => (BinaryPrimitives.ReadDoubleBigEndian(data.Slice(start + 1, 8)), start + 9),
                'C' => ((char) data[start + 1], start + 2),
                _ => throw new NotImplementedException($"Unhandled type {data[start]:x2}='{(char) data[start]}'")
            };

        (string, int) ReadString(ReadOnlySpan<byte> data, int startPastType)
        {
            var (obj, textStart) = ReadValue(data, startPastType);
            int length = (int) obj!;
            return (Encoding.UTF8.GetString(data.Slice(textStart, length)), textStart + length);
        }
    }
}
