// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace CameraControl.Visca;

/// <summary>
/// A VISCA message, containing a payload and potentially a sequence number and
/// message type.
/// </summary>
internal readonly record struct ViscaMessage(ViscaMessageType? Type, int? SequenceNumber, ViscaPayload Payload)
{
    internal ViscaMessage(ViscaPayload payload) : this(null, null, payload)
    {
    }

    internal ViscaMessage(ViscaMessageType type, params byte[] payload) : this(type, null, ViscaPayload.FromBytes(payload).WithPreformatting())
    {
    }

    private bool IncludeHeader => SequenceNumber is not null && Type is not null;

    /// <summary>
    /// The total length of this message in bytes, including the header
    /// if both <see cref="SequenceNumber"/> and <see cref="Type"/> are non-null.
    /// </summary>
    public int Length => Payload.Length + (IncludeHeader ? 8 : 0);

    internal void WriteTo(Span<byte> buffer)
    {
        if (Length > buffer.Length)
        {
            throw new ArgumentException($"Buffer does not have enough space; {Length} > {buffer.Length}");
        }
        if (IncludeHeader)
        {
            int type = (int) Type!.Value;
            buffer[0] = (byte) (type >> 8);
            buffer[1] = (byte) type;
            buffer[2] = (byte) (Payload.Length >> 8);
            buffer[3] = (byte) Payload.Length;
            long seq = SequenceNumber!.Value;
            buffer[4] = (byte) (seq >> 24);
            buffer[5] = (byte) (seq >> 16);
            buffer[6] = (byte) (seq >> 8);
            buffer[7] = (byte) seq;
            buffer = buffer.Slice(8);
        }
        Payload.WriteTo(buffer);
    }

    internal static ViscaMessage? Parse(ReadOnlySpan<byte> data, ViscaMessageFormat format) =>
        format switch
        {
            ViscaMessageFormat.Raw => ParseRaw(data),
            ViscaMessageFormat.Encapsulated => ParseEncapsulated(data),
            _ => throw new ArgumentException($"Unknown message format '{format}'")
        };

    internal static ViscaMessage? ParseRaw(ReadOnlySpan<byte> data)
    {
        int index = data.IndexOf((byte) 0xff);
        return index == -1 ? null : new ViscaMessage(ViscaPayload.FromSpan(data.Slice(0, index + 1)));
    }

    internal static ViscaMessage? ParseEncapsulated(ReadOnlySpan<byte> data)
    {
        if (data.Length < 10)
        {
            return null;
        }
        int length = (data[2] << 8) | data[3];
        if (data.Length < length + 8)
        {
            return null;
        }
        int seq = (data[4] << 24) |
            (data[5] << 16) |
            (data[6] << 8) |
            data[7];
        int type = (data[0] << 8) | data[1];
        // TODO: Validate that the payload ends with FF?
        return new ViscaMessage((ViscaMessageType) type, seq, ViscaPayload.FromSpan(data.Slice(8, length)));
    }
}
