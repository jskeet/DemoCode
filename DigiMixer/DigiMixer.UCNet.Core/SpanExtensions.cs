using System.Text;

namespace DigiMixer.UCNet.Core;

public static class SpanExtensions
{
    public static void WriteUInt16(this Span<byte> span, ushort value)
    {
        if (!BitConverter.TryWriteBytes(span, value))
        {
            throw new ArgumentException("Invalid length of span");
        }
    }

    public static void WriteUInt32(this Span<byte> span, uint value)
    {
        if (!BitConverter.TryWriteBytes(span, value))
        {
            throw new ArgumentException("Invalid length of span");
        }
    }

    public static void WriteInt32(this Span<byte> span, int value)
    {
        if (!BitConverter.TryWriteBytes(span, value))
        {
            throw new ArgumentException("Invalid length of span");
        }
    }

    public static int WriteString(this Span<byte> span, string value) =>
        Encoding.UTF8.GetBytes(value, span);

    public static void WriteBytes(this Span<byte> destination, byte[] source)
    {
        source.CopyTo(destination);
    }

    public static ushort ReadUInt16(this ReadOnlySpan<byte> span) =>
        BitConverter.ToUInt16(span);

    public static uint ReadUInt32(this ReadOnlySpan<byte> span) =>
        BitConverter.ToUInt32(span);

    public static int ReadInt32(this ReadOnlySpan<byte> span) =>
        BitConverter.ToInt32(span);

    public static string ReadString(this ReadOnlySpan<byte> span) =>
        Encoding.UTF8.GetString(span);
}
