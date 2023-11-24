// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Text;

namespace CameraControl.Visca;

/// <summary>
/// VISCA payload, which may be either a complete message in itself,
/// or have additional headers (Sony-style protocol).
/// </summary>
internal readonly struct ViscaPayload
{
    // VISCA packets can contain up to 16 bytes of payload.
    // It's simplest to represent those as two long values.
    // TODO: Use Int128 when we've moved to .NET 8+.
    private readonly long head;
    private readonly long tail;

    /// <summary>
    /// The text representation, cached for efficiency for commonly-used packets.
    /// </summary>
    private readonly string? text;

    /// <summary>
    /// The number of bytes in the packet, including the trailing 0xFF.
    /// </summary>
    public int Length { get; }

    private ViscaPayload(long head, long tail, int length, string? text) =>
        (this.head, this.tail, this.Length, this.text) = (head, tail, length, text);

    internal static ViscaPayload FromBytes(params byte[] bytes) =>
        FromSpan(Preconditions.CheckNotNull(bytes));

    internal ViscaPayload WithPreformatting() =>
        new ViscaPayload(head, tail, Length, ToString());

    internal static ViscaPayload FromSpan(ReadOnlySpan<byte> bytes)
    {
        int length = bytes.Length;
        long head = GetByteOrZero(0, 56, bytes) |
            GetByteOrZero(1, 48, bytes) |
            GetByteOrZero(2, 40, bytes) |
            GetByteOrZero(3, 32, bytes) |
            GetByteOrZero(4, 24, bytes) |
            GetByteOrZero(5, 16, bytes) |
            GetByteOrZero(6, 8, bytes) |
            GetByteOrZero(7, 0, bytes);

        long tail = GetByteOrZero(8, 56, bytes) |
            GetByteOrZero(9, 48, bytes) |
            GetByteOrZero(10, 40, bytes) |
            GetByteOrZero(11, 32, bytes) |
            GetByteOrZero(12, 24, bytes) |
            GetByteOrZero(13, 16, bytes) |
            GetByteOrZero(14, 8, bytes) |
            GetByteOrZero(15, 0, bytes);

        return new ViscaPayload(head, tail, length, null);

        long GetByteOrZero(int index, int shift, ReadOnlySpan<byte> bytes) =>
            index >= length ? 0L : ((long) bytes[index]) << shift;
    }

    // While this could be an indexer, making it a method allows the
    // GetInt16 method to be accessed in a consistent manner.
    internal byte GetByte(int index)
    {
        Preconditions.CheckRange(index, 0, Length);
        return index < 8
            ? (byte) (head >> (56 - (index * 8)))
            : (byte) (tail >> (120 - (index * 8)));
    }

    internal short GetInt16(int index)
    {
        Preconditions.CheckRange(index, 0, Length - 4);
        return (short) (
            (GetByte(index) << 12) |
            (GetByte(index + 1) << 8) |
            (GetByte(index + 2) << 4) |
            (GetByte(index + 3) << 0)
        );
    }

    internal ViscaPayload WithInt16Set(int index, short value)
    {
        long newHead = head;
        long newTail = tail;

        SetByte(index, (byte) ((value >> 12) & 0xf), ref newHead, ref newTail);
        SetByte(index + 1, (byte) ((value >> 8) & 0xf), ref newHead, ref newTail);
        SetByte(index + 2, (byte) ((value >> 4) & 0xf), ref newHead, ref newTail);
        SetByte(index + 3, (byte) ((value >> 0) & 0xf), ref newHead, ref newTail);
        return new ViscaPayload(newHead, newTail, Length, null);
    }

    internal ViscaPayload WithByteSet(int index, byte value)
    {
        long newHead = head;
        long newTail = tail;

        SetByte(index, value, ref newHead, ref newTail);
        return new ViscaPayload(newHead, newTail, Length, null);
    }

    private void SetByte(int index, byte value, ref long newHead, ref long newTail)
    {
        if (index < 0 || index >= Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        ref long x = ref index < 8 ? ref newHead : ref newTail;
        int shift = (7 - (index & 7)) * 8;
        long mask = 0xffL << shift;
        x &= ~mask;
        x |= ((long) value) << shift;
    }

    public override string ToString()
    {
        if (text is not null)
        {
            return text;
        }
        var builder = new StringBuilder();
        for (int i = 0; i < Length; i++)
        {
            builder.AppendFormat("{0:x2}", GetByte(i));
            builder.Append('-');
        }
        builder.Length--;
        return builder.ToString();
    }

    internal void WriteTo(Span<byte> buffer)
    {
        if (Length > buffer.Length)
        {
            throw new ArgumentException($"Buffer does not have enough space; {Length} > {buffer.Length}");
        }
        for (int i = 0; i < Length; i++)
        {
            buffer[i] = GetByte(i);
        }
    }

    /// <summary>
    /// Convenience method (for chaining) to create a message with a type of Command from this payload.
    /// </summary>
    internal ViscaMessage ToCommandMessage() => new ViscaMessage(ViscaMessageType.Command, null, this);
}
