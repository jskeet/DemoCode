using DigiMixer.Core;
using System.Buffers.Binary;

namespace DigiMixer.AllenAndHeath.Core;

/// <summary>
/// A raw, uninterpreted (other than format and type) Allen and Heath message.
/// </summary>
public sealed class AHRawMessage : IMixerMessage<AHRawMessage>
{
    private const byte VariableLengthPrefix = 0x7f;
    private const byte FixedLengthPrefix = 0xf7;

    public AHMessageFormat Format { get; }

    /// <summary>
    /// The message type, which is null if and only if the format is FixedLength8 or FixedLength9.
    /// </summary>
    public byte? Type { get; }

    private ReadOnlyMemory<byte> data;

    public ReadOnlySpan<byte> Data => data.Span;

    private AHRawMessage(AHMessageFormat format, byte? type, ReadOnlyMemory<byte> data)
    {
        Format = format;
        Type = type;
        this.data = data;
    }

    public static AHRawMessage ForFixedLength(ReadOnlyMemory<byte> data)
    {
        var format = data.Length switch
        {
            7 => AHMessageFormat.FixedLength8,
            8 => AHMessageFormat.FixedLength9,
            _ => throw new ArgumentException()
        };
        return new(format, null, data);
    }

    public static AHRawMessage ForVariableLength(byte type, ReadOnlyMemory<byte> data) =>
        new(AHMessageFormat.VariableLength, type, data);

    /// <summary>
    /// Length of the total message, including header.
    /// </summary>
    public int Length => Format switch
    {
        AHMessageFormat.VariableLength => Data.Length + 6,
        AHMessageFormat.FixedLength8 => 8,
        AHMessageFormat.FixedLength9 => 9,
        _ => throw new InvalidOperationException()
    };

    public static AHRawMessage? TryParse(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
        {
            return null;
        }
        return data[0] switch
        {
            VariableLengthPrefix => TryParseVariableLength(data.ToArray()),
            FixedLengthPrefix => TryParseFixedLength(data.ToArray()),
            _ => throw new ArgumentException($"Invalid data: first byte is 0x{data[0]:x2}")
        };
    }

    private static AHRawMessage? TryParseVariableLength(ReadOnlyMemory<byte> data)
    {
        if (data.Length < 6)
        {
            return null;
        }
        byte type = data.Span[1];
        int dataLength = BinaryPrimitives.ReadInt32LittleEndian(data[2..6].Span);
        if (data.Length < dataLength + 6)
        {
            return null;
        }
        return new AHRawMessage(AHMessageFormat.VariableLength, type, data[6..(dataLength + 6)].ToArray());
    }

    private static AHRawMessage? TryParseFixedLength(ReadOnlyMemory<byte> data)
    {
        if (data.Length < 8)
        {
            return null;
        }
        // Last of these has only been seen on the SQ...
        if ((data.Span[1] == 0x12 && data.Span[3] == 0x23) ||
            (data.Span[1] == 0x13 && data.Span[3] == 0x16) ||
            (data.Span[1] == 0x1a && data.Span[2] == 0x1a && data.Span[3] == 0x26))
        {
            if (data.Length < 9)
            {
                return null;
            }
            return ForFixedLength(data[1..9]);
        }

        return ForFixedLength(data[1..8]);
    }

    public override string ToString() => $"Type={Type}; Length={Data.Length}";

    public void CopyTo(Span<byte> buffer)
    {
        switch (Format)
        {
            case AHMessageFormat.VariableLength:
                buffer[0] = VariableLengthPrefix;
                buffer[1] = Type!.Value;
                BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(2, 4), Data.Length);
                data.Span.CopyTo(buffer.Slice(6));
                break;
            case AHMessageFormat.FixedLength8:
            case AHMessageFormat.FixedLength9:
                buffer[0] = FixedLengthPrefix;
                data.Span.CopyTo(buffer.Slice(1));
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}
