using DigiMixer.Core;
using System.Data;
using System.Text;

namespace DigiMixer.Diagnostics;

/// <summary>
/// Utilities for hex parsing/formatting.
/// </summary>
public class Hex
{
    /// <summary>
    /// Formats arbitrary data as lines of hex.
    /// </summary>
    /// <param name="data">The data to format.</param>
    /// <param name="lineLength">The line length, in bytes</param>
    public static string[] ConvertAndSplit(ReadOnlySpan<byte> data, int lineLength, int? extraSpaceAt = null)
    {
        var lines = new string[(data.Length + lineLength - 1) / lineLength];
        for (int i = 0; i < lines.Length; i++)
        {
            int offset = i * lineLength;
            var slice = data.Slice(offset, Math.Min(lineLength, data.Length - offset));
            var builder = new StringBuilder();
            builder.Append(offset.ToString("x4"));
            builder.Append("  ");
            for (int index = 0; index < slice.Length; index++)
            {
                if (extraSpaceAt == index)
                {
                    builder.Append(' ');
                }
                builder.Append(slice[index].ToString("x2"));
                builder.Append(' ');
            }
            lines[i] = builder.ToString();
        }
        return lines;
    }

    /// <summary>
    /// Parses a line from a hex dump, expecting a format as per a Wireshark
    /// "follow stream" hex dump.
    /// </summary>
    public static HexDumpLine ParseHexDumpLine(string line)
    {
        var direction = Direction.Outbound;
        if (line.StartsWith("    "))
        {
            direction = Direction.Inbound;
            line = line[4..];
        }
        ulong offset = Convert.ToUInt64(line[0..8], 16);
        var dataText = line[10..];
        int end = dataText.IndexOf("   ");
        dataText = dataText[0..end].Replace(" ", "");
        byte[] data = new byte[dataText.Length / 2];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Convert.ToByte(dataText[(i * 2)..(i * 2 + 2)], 16);
        }
        return new HexDumpLine(direction, offset, data);
    }

    /// <summary>
    /// Parses hex values, ignoring any spaces.
    /// </summary>
    public static byte[] ParseHex(string text)
    {
        text = text.Replace(" ", "");
        byte[] data = new byte[text.Length / 2];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Convert.ToByte(text[(i * 2)..(i * 2 + 2)], 16);
        }
        return data;
    }

    public record HexDumpLine(Direction Direction, ulong Offset, byte[] Data)
    {
        public override string ToString() =>
            $"{Direction:8}: {Offset:x8}: {Formatting.ToHex(Data)}";
    }

    /// <summary>
    /// We don't really know for any given dump whether the indented or
    /// non-indented traffic is the mixer to client or vice versa. We'll
    /// call non-indented "outbound" and indented "inbound".
    /// </summary>
    public enum Direction
    {
        Outbound, Inbound
    }
}
