using DigiMixer.Core;
using System.Buffers.Binary;

namespace DigiMixer.CqSeries.Core;

/// <summary>
/// A raw, uninterpreted (other than format and type) CQ message.
/// </summary>
public sealed class CqRawMessage : IMixerMessage<CqRawMessage>
{
    private const byte VariableLengthPrefix = 0x7f;
    private const byte FixedLengthPrefix = 0xf7;

    public CqMessageFormat Format { get; }
    public CqMessageType Type { get; }

    private ReadOnlyMemory<byte> data;

    public ReadOnlySpan<byte> Data => data.Span;

    public CqRawMessage(CqMessageFormat format, CqMessageType type, byte[] data)
    {
        Format = format;
        Type = type;
        this.data = data ?? throw new ArgumentNullException(nameof(data));
    }
    
    /// <summary>
    /// Length of the total message, including header.
    /// </summary>
    public int Length => Format switch
    {
        CqMessageFormat.VariableLength => Data.Length + 6,
        CqMessageFormat.FixedLength8 => 8,
        CqMessageFormat.FixedLength9 => 9,
        _ => throw new InvalidOperationException()
    };

    public static CqRawMessage? TryParse(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
        {
            return null;
        }
        return data[0] switch
        {
            VariableLengthPrefix => TryParseVariableLength(data),
            FixedLengthPrefix => TryParseFixedLength(data),
            _ => throw new ArgumentException($"Invalid data: first byte is 0x{data[0]:x2}")
        };
    }

    private static CqRawMessage? TryParseVariableLength(ReadOnlySpan<byte> data)
    {
        if (data.Length < 6)
        {
            return null;
        }
        CqMessageType type = (CqMessageType) data[1];
        int dataLength = BinaryPrimitives.ReadInt32LittleEndian(data[2..6]);
        if (data.Length < dataLength + 6)
        {
            return null;
        }
        return new CqRawMessage(CqMessageFormat.VariableLength, type, data[6..(dataLength + 6)].ToArray());
    }

    private static CqRawMessage? TryParseFixedLength(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8)
        {
            return null;
        }
        if ((data[1] == 0x12 && data[3] == 0x23) || (data[1] == 0x13 && data[3] == 0x16))
        {
            if (data.Length < 9)
            {
                return null;
            }
            return new CqRawMessage(CqMessageFormat.FixedLength9, CqMessageType.Regular, data[1..9].ToArray());
        }
        
        return new CqRawMessage(CqMessageFormat.FixedLength8, CqMessageType.Regular, data[1..8].ToArray());
    }

    public override string ToString() => $"Type={Type}; Length={Data.Length}";

    public void CopyTo(Span<byte> buffer)
    {
        switch (Format)
        {
            case CqMessageFormat.VariableLength:
                buffer[0] = VariableLengthPrefix;
                buffer[1] = (byte) Type;
                BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(2, 4), Data.Length);
                data.Span.CopyTo(buffer.Slice(6));
                break;
            case CqMessageFormat.FixedLength8:
            case CqMessageFormat.FixedLength9:
                buffer[0] = FixedLengthPrefix;
                data.Span.CopyTo(buffer.Slice(1));
                break;
            default:
                throw new InvalidOperationException();
        }

    }
}
