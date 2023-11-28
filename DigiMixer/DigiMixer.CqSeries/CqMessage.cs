using DigiMixer.CqSeries.Core;
using System.Text;

namespace DigiMixer.CqSeries;

/// <summary>
/// Base class for type-specific messages. These wrap <see cref="CqRawMessage"/>.
/// </summary>
public abstract class CqMessage
{
    public CqRawMessage RawMessage { get; }

    public ReadOnlySpan<byte> Data => RawMessage.Data;
    public CqMessageType Type => RawMessage.Type;

    public override string ToString() => $"Type={Type}; Length={Data.Length}";

    protected CqMessage(CqRawMessage rawMessage)
    {
        RawMessage = rawMessage;
    }

    protected CqMessage(CqMessageFormat format, CqMessageType type, byte[] data) : this(new CqRawMessage(format, type, data))
    {
    }

    internal ushort GetUInt16(int index) => (ushort) (Data[index] | (Data[index + 1] << 8));

    internal string? GetString(int offset, int maxLength)
    {
        int length = Data.Slice(offset, maxLength).IndexOf((byte) 0);
        if (length == -1)
        {
            length = maxLength;
        }
        return length == 0 ? null : Encoding.ASCII.GetString(Data.Slice(offset, length));
    }

    public static CqMessage FromRawMessage(CqRawMessage rawMessage) => rawMessage.Type switch
    {
        CqMessageType.UdpHandshake => new CqUdpHandshakeMessage(rawMessage),
        CqMessageType.Regular => new CqRegularMessage(rawMessage),
        CqMessageType.KeepAlive => new CqKeepAliveMessage(rawMessage),
        CqMessageType.VersionRequest => new CqVersionRequestMessage(rawMessage),
        CqMessageType.VersionResponse => new CqVersionResponseMessage(rawMessage),
        CqMessageType.ClientInitRequest => new CqClientInitRequestMessage(rawMessage),
        CqMessageType.ClientInitResponse => new CqClientInitResponseMessage(rawMessage),
        CqMessageType.FullDataRequest => new CqFullDataRequestMessage(rawMessage),
        CqMessageType.FullDataResponse => new CqFullDataResponseMessage(rawMessage),
        _ => new CqUnknownMessage(rawMessage)
    };
}
