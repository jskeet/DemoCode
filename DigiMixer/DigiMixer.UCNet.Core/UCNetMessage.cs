using DigiMixer.UCNet.Core.Messages;

namespace DigiMixer.UCNet.Core;

// Packet structure:
// - Header:
//   - Magic Number (55 43 00 01)
//   - Size (2 bytes) - includes type, mode and body
//   - Type (2 bytes)
//   - Mode (4 bytes)
// - Body

public abstract class UCNetMessage
{
    private static readonly byte[] MagicNumber = { 0x55, 0x43, 0x00, 0x01 };

    // TODO: Give this a better name (and maybe use an enum)
    public MessageMode Mode { get; }

    /// <summary>
    /// Length of the whole message, including the header.
    /// </summary>
    public int Length => BodyLength + 12;

    public abstract MessageType Type { get; }
    protected abstract int BodyLength { get; }

    protected UCNetMessage(MessageMode mode)
    {
        Mode = mode;
    }

    internal byte[] ToByteArray()
    {
        byte[] array = new byte[Length];
        var span = array.AsSpan();
        span.WriteBytes(MagicNumber);
        span.Slice(4).WriteUInt16((ushort) (BodyLength + 6));
        span.Slice(6).WriteUInt16((ushort) Type);
        span.Slice(8).WriteUInt32((uint) Mode);
        WriteBody(span.Slice(12));
        return array;
    }

    protected abstract void WriteBody(Span<byte> span);

    /// <summary>
    /// Messages over UDP have the port instead of the length as bytes 4 & 5; the length is assumed to be the whole packet.
    /// </summary>
    internal static UCNetMessage? TryParseUdp(ReadOnlySpan<byte> span)
    {
        if (span.Length < 6)
        {
            return null;
        }
        for (int i = 0; i < MagicNumber.Length; i++)
        {
            if (span[i] != MagicNumber[i])
            {
                throw new InvalidDataException($"Header byte {i} is incorrect: expected {MagicNumber[i]:x2}; was {span[i]:x2}");
            }
        }

        // Skip port (bytes 4 and 5)
        var length = span.Length - 6;

        var type = (MessageType) span.Slice(6).ReadUInt16();
        var mode = (MessageMode) span.Slice(8).ReadUInt32();
        var body = span[12..(length + 6)];
        return type switch
        {
            MessageType.UdpMeters => UdpMetersMessage.FromRawBody(mode, body),
            MessageType.Json => JsonMessage.FromRawBody(mode, body),
            MessageType.CompressedJson => CompressedJsonMessage.FromRawBody(mode, body),
            MessageType.FileRequest => FileRequestMessage.FromRawBody(mode, body),
            MessageType.ParameterValue => ParameterValueMessage.FromRawBody(mode, body),
            MessageType.ParameterString => ParameterStringMessage.FromRawBody(mode, body),
            MessageType.KeepAlive => KeepAliveMessage.FromRawBody(mode, body),
            MessageType.BinaryObject => BinaryObjectMessage.FromRawBody(mode, body),
            MessageType.Chunk => ChunkMessage.FromRawBody(mode, body),
            MessageType.ParameterStringList => ParameterStringListMessage.FromRawBody(mode, body),
            MessageType.Meter16 => Meter16Message.FromRawBody(mode, body),
            MessageType.FileData => FileDataMessage.FromRawBody(mode, body),
            MessageType.Meter8 => Meter8Message.FromRawBody(mode, body),
            _ => throw new NotSupportedException($"Packet type {type} not currently supported")
        };

    }

    internal static UCNetMessage? TryParse(ReadOnlySpan<byte> span)
    {
        if (span.Length < 6)
        {
            return null;
        }
        for (int i = 0; i < MagicNumber.Length; i++)
        {
            if (span[i] != MagicNumber[i])
            {
                throw new InvalidDataException($"Header byte {i} is incorrect: expected {MagicNumber[i]:x2}; was {span[i]:x2}");
            }
        }
        var length = span.Slice(4).ReadUInt16();
        if (length > span.Length - 6)
        {
            return null;
        }
        var type = (MessageType) span.Slice(6).ReadUInt16();
        var mode = (MessageMode) span.Slice(8).ReadUInt32();
        var body = span[12..(length + 6)];
        return type switch
        {
            MessageType.UdpMeters => UdpMetersMessage.FromRawBody(mode, body),
            MessageType.Json => JsonMessage.FromRawBody(mode, body),
            MessageType.CompressedJson => CompressedJsonMessage.FromRawBody(mode, body),
            MessageType.FileRequest => FileRequestMessage.FromRawBody(mode, body),
            MessageType.ParameterValue => ParameterValueMessage.FromRawBody(mode, body),
            MessageType.ParameterString => ParameterStringMessage.FromRawBody(mode, body),
            MessageType.KeepAlive => KeepAliveMessage.FromRawBody(mode, body),
            MessageType.BinaryObject => BinaryObjectMessage.FromRawBody(mode, body),
            MessageType.Chunk => ChunkMessage.FromRawBody(mode, body),
            MessageType.ParameterStringList => ParameterStringListMessage.FromRawBody(mode, body),
            MessageType.Meter16 => Meter16Message.FromRawBody(mode, body),
            MessageType.FileData => FileDataMessage.FromRawBody(mode, body),
            MessageType.Meter8 => Meter8Message.FromRawBody(mode, body),
            _ => throw new NotSupportedException($"Packet type {type} not currently supported")
        };
    }
}
