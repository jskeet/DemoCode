namespace DigiMixer.Core;

/// <summary>
/// Formatting utility methods.
/// </summary>
public static class Formatting
{
    private const string HexDigits = "0123456789ABCDEF";

    public static string ToHex(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
        {
            return "";
        }
        char[] chars = new char[data.Length * 3 - 1];
        for (int i = 0; i < data.Length; i++)
        {
            if (i != 0)
            {
                chars[i * 3 - 1] = ' ';
            }
            chars[i * 3] = HexDigits[(data[i] & 0xf0) >> 4];
            chars[i * 3 + 1] = HexDigits[data[i] & 0x0f];
        }
        return new string(chars);
    }

    // Slightly optimized over the overload above, as we don't need to allocate
    // a char array first; we can write straight into the string via a span.
    public static string ToHex(byte[] data)
    {
        if (data.Length == 0)
        {
            return "";
        }
        return string.Create(data.Length * 3 - 1, data, FormatData);

        static void FormatData(Span<char> chars, byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (i != 0)
                {
                    chars[i * 3 - 1] = ' ';
                }
                chars[i * 3] = HexDigits[(data[i] & 0xf0) >> 4];
                chars[i * 3 + 1] = HexDigits[data[i] & 0x0f];
            }
        }
    }
}
