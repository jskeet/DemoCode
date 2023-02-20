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
    public static string[] ConvertAndSplit(ReadOnlySpan<byte> data, int lineLength)
    {
        var lines = new string[(data.Length + lineLength - 1) / lineLength];
        for (int i = 0; i < lines.Length; i++)
        {
            int offset = i * lineLength;
            var slice = data.Slice(offset, Math.Min(lineLength, data.Length - offset));
            var builder = new StringBuilder();
            builder.Append(offset.ToString("x4"));
            builder.Append("  ");
            foreach (var b in slice)
            {
                builder.Append(b.ToString("x2"));
                builder.Append(' ');
            }
            lines[i] = builder.ToString();
        }
        return lines;
    }
}
