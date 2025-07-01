using DigiMixer.AllenAndHeath.Core;
using DigiMixer.Core;
using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.SqSeries;

/// <summary>
/// Base class for type-specific messages. These wrap <see cref="AHRawMessage"/>.
/// </summary>
public abstract class SqMessage(AHRawMessage rawMessage)
{
    public AHRawMessage RawMessage { get; } = rawMessage;

    public ReadOnlySpan<byte> Data => RawMessage.Data;
    public SqMessageType? Type => (SqMessageType?) RawMessage.Type;

    public override string ToString() => Type is null ? $"Fixed: {Formatting.ToHex(Data)}" : $"Type={Type}; Length={Data.Length}";

    protected SqMessage(SqMessageType type, byte[] data) : this(AHRawMessage.ForVariableLength((byte) type, data))
    {
    }

    internal ushort GetUInt16(int index) => BinaryPrimitives.ReadUInt16LittleEndian(Data[index..]);

    internal string? GetString(int offset, int maxLength)
    {
        int length = Data.Slice(offset, maxLength).IndexOf((byte) 0);
        if (length == -1)
        {
            length = maxLength;
        }
        return length == 0 ? null : Encoding.ASCII.GetString(Data.Slice(offset, length));
    }

    public static SqMessage FromRawMessage(AHRawMessage rawMessage) => (SqMessageType?) rawMessage.Type switch
    {
        SqMessageType.UdpHandshake => new SqUdpHandshakeMessage(rawMessage),
        //SqMessageType.Regular => new SqRegularMessage(rawMessage),
        //SqMessageType.KeepAlive => new SqKeepAliveMessage(rawMessage),
        SqMessageType.VersionRequest => new SqVersionRequestMessage(rawMessage),
        SqMessageType.VersionResponse => new SqVersionResponseMessage(rawMessage),
        SqMessageType.ClientInitRequest => new SqClientInitRequestMessage(rawMessage),
        SqMessageType.ClientInitResponse => new SqClientInitResponseMessage(rawMessage),
        SqMessageType.FullDataRequest => new SqSimpleRequestMessage(rawMessage),
        SqMessageType.Type15Request => new SqSimpleRequestMessage(rawMessage),
        SqMessageType.Type17Request => new SqSimpleRequestMessage(rawMessage),
        //SqMessageType.FullDataResponse => new SqFullDataResponseMessage(rawMessage),
        //SqMessageType.InputMeters => new SqInputMetersMessage(rawMessage),
        //SqMessageType.OutputMeters => new SqOutputMetersMessage(rawMessage),
        _ => new SqUnknownMessage(rawMessage)
    };
}
