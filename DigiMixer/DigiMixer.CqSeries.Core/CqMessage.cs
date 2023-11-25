using System.Runtime.CompilerServices;

namespace DigiMixer.CqSeries.Core;

public abstract class CqMessage
{
    private const byte VariableLengthPrefix = 0x7f;
    private const byte FixedLengthPrefix = 0xf7;

    public CqMessageFormat Format { get; }
    public abstract CqMessageType Type { get; }

    private ReadOnlyMemory<byte> data;

    public ReadOnlySpan<byte> Data => data.Span;

    protected CqMessage(CqMessageFormat format, byte[] data)
    {
        Format = format;
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

    internal ushort GetUInt16(int index) => (ushort) (Data[index] | (Data[index + 1] << 8));

    public static CqMessage? TryParse(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
        {
            return null;
        }
        return data[0] switch
        {
            VariableLengthPrefix => TryParseVariableLength(data),
            FixedLengthPrefix => TryParseFixedLength(data),
            _ => throw new ArgumentException($"Invalid data: first byte is {data[0]:x2}")
        };
    }

    private static CqMessage? TryParseVariableLength(ReadOnlySpan<byte> data)
    {
        if (data.Length < 6)
        {
            return null;
        }
        CqMessageType type = (CqMessageType) data[1];
        int dataLength = BitConverter.ToInt32(data[2..6]);
        if (data.Length < dataLength + 6)
        {
            return null;
        }
        return CreateMessage(CqMessageFormat.VariableLength, type, data[6..(dataLength + 6)].ToArray());
    }

    private static CqMessage? TryParseFixedLength(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8)
        {
            return null;
        }
        if (data[1] == 0x12 || data[1] == 0x13)
        {
            if (data.Length < 9)
            {
                return null;
            }
            return CreateMessage(CqMessageFormat.FixedLength9, CqMessageType.Regular, data[1..9].ToArray());
        }
        
        return CreateMessage(CqMessageFormat.FixedLength8, CqMessageType.Regular, data[1..8].ToArray());
    }

    private static CqMessage CreateMessage(CqMessageFormat format, CqMessageType type, byte[] data) => type switch
    {
        CqMessageType.Handshake => new CqHandshakeMessage(format, data),
        CqMessageType.Regular => new CqRegularMessage(format, data),
        CqMessageType.KeepAlive => new CqKeepAliveMessage(format, data),
        CqMessageType.AllDataRequest => new CqAllDataRequestMessage(format, data),
        CqMessageType.AllDataResponse => new CqAllDataResponseMessage(format, data),
        _ => new CqUnknownMessage(format, type, data)
    };

    public byte[] ToByteArray()
    {
        byte[] ret = new byte[Length];
        switch (Format)
        {
            case CqMessageFormat.VariableLength:
                ret[0] = VariableLengthPrefix;
                ret[1] = (byte) Type;
                BitConverter.TryWriteBytes(ret.AsSpan().Slice(2, 4), Data.Length);
                data.CopyTo(ret.AsMemory().Slice(6));
                break;
            case CqMessageFormat.FixedLength8:
            case CqMessageFormat.FixedLength9:
                ret[0] = FixedLengthPrefix;
                data.CopyTo(ret.AsMemory().Slice(1));
                break;
            default:
                throw new InvalidOperationException();
        }
        return ret;
    }

    public override string ToString() => $"Type={Type}; Length={Data.Length}";
}
