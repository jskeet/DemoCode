using System.Runtime.InteropServices;

namespace DigiMixer.Core;

/// <summary>
/// Read and write operations for big-endian formats.
/// Reminder: in big-endian format, the most significant byte comes
/// first, so a 16-bit unsigned integer with value 260 would be 0x01 0x04.
/// </summary>
public static class BigEndian
{
    // Validate that we're really on a big-endian machine...
    // TODO: Instead of validating, do the right thing either way.
    // (The endian classes have been designed to make that a localized change.)
    static BigEndian()
    {
        byte[] bytes = [0, 1];
        if (ReadUInt16(bytes) != 1)
        {
            throw new InvalidOperationException("Unexpected underlying endianness");
        }
    }

    public static ushort ReadUInt16(ReadOnlySpan<byte> source)
    {
        Span<byte> reversed = stackalloc byte[2];
        reversed[0] = source[1];
        reversed[1] = source[0];
        return MemoryMarshal.Read<ushort>(reversed);
    }

    public static void WriteUInt16(Span<byte> destination, ushort value)
    {
        Span<byte> reversed = stackalloc byte[2];
        MemoryMarshal.Write(reversed, in value);
        destination[0] = reversed[1];
        destination[1] = reversed[0];
    }

    public static uint ReadUInt32(ReadOnlySpan<byte> source)
    {
        Span<byte> reversed = stackalloc byte[4];
        reversed[0] = source[3];
        reversed[1] = source[2];
        reversed[2] = source[1];
        reversed[3] = source[0];
        return MemoryMarshal.Read<uint>(reversed);
    }

    public static void WriteUInt32(Span<byte> destination, uint value)
    {
        Span<byte> reversed = stackalloc byte[4];
        MemoryMarshal.Write(reversed, in value);
        destination[0] = reversed[3];
        destination[1] = reversed[2];
        destination[2] = reversed[1];
        destination[3] = reversed[0];
    }

    public static short ReadInt16(ReadOnlySpan<byte> source) =>
        unchecked((short) ReadUInt16(source));

    public static void WriteInt16(Span<byte> destination, short value) =>
        WriteUInt16(destination, unchecked((ushort) value));

    public static int ReadInt32(ReadOnlySpan<byte> source) =>
        unchecked((int) ReadUInt32(source));

    public static void WriteInt32(Span<byte> destination, int value) =>
        WriteUInt32(destination, unchecked((uint) value));
}
