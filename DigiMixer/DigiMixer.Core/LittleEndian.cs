using System.Runtime.InteropServices;

namespace DigiMixer.Core;

/// <summary>
/// Read and write operations for little-endian formats.
/// Reminder: in little-endian format, the least significant byte comes
/// first, so a 16-bit unsigned integer with value 260 would be 0x04 0x01.
/// </summary>
public static class LittleEndian
{
    // Validate that we're really on a little-endian machine...
    // TODO: Instead of validating, do the right thing either way.
    // (The endian classes have been designed to make that a localized change.)
    static LittleEndian()
    {
        byte[] bytes = [0, 1];
        if (ReadUInt16(bytes) != 256)
        {
            throw new InvalidOperationException("Unexpected underlying endianness");
        }
    }
    public static ushort ReadUInt16(ReadOnlySpan<byte> source) =>
        MemoryMarshal.Read<ushort>(source);

    public static void WriteUInt16(Span<byte> destination, ushort value) =>
        MemoryMarshal.Write(destination, in value);

    public static uint ReadUInt32(ReadOnlySpan<byte> source) =>
        MemoryMarshal.Read<uint>(source);

    public static void WriteUInt32(Span<byte> destination, uint value) =>
        MemoryMarshal.Write(destination, in value);

    public static short ReadInt16(ReadOnlySpan<byte> source) =>
        unchecked((short) ReadUInt16(source));

    public static void WriteInt16(Span<byte> destination, short value) =>
        WriteUInt16(destination, unchecked((ushort) value));

    public static int ReadInt32(ReadOnlySpan<byte> source) =>
        unchecked((int) ReadUInt32(source));

    public static void WriteInt32(Span<byte> destination, int value) =>
        WriteUInt32(destination, unchecked((uint) value));
}
