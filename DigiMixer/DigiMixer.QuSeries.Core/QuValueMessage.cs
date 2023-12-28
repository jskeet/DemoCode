using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace DigiMixer.QuSeries.Core;

/// <summary>
/// A message with a single 16-bit value, either directly from a client to the mixer,
/// or from the mixer broadcasting a client's message to other clients.
/// </summary>
public class QuValueMessage : QuControlMessage
{
    public byte ClientId { get; }
    public byte Section { get; }
    public ushort RawValue { get; }
    public int Address { get; }

    public QuValueMessage(byte clientId, byte section, int address, ushort rawValue) =>
        (ClientId, Section, Address, RawValue) = (clientId, section, address, rawValue);

    internal QuValueMessage(ReadOnlySpan<byte> data) : this(
        data[0],
        data[1],
        MemoryMarshal.Cast<byte, int>(data.Slice(2))[0],
        MemoryMarshal.Cast<byte, ushort>(data.Slice(6))[0])
    {
    }

    public override int Length => 9;

    public override void CopyTo(Span<byte> buffer)
    {
        buffer[0] = 0xf7;
        buffer[1] = ClientId;
        buffer[2] = Section;
        // Note offsets of 3 and 7 rather than 2 and 6, as the message includes the fixed-length indicator.
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(3), Address);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(7), RawValue);
    }

    public override string ToString() =>
        $"Value: Client: {ClientId:X2}; Section: {Section:X2}; Address: {Address:X8}; RawValue: {RawValue:X4}";
}
