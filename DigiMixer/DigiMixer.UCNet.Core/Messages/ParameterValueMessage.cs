using DigiMixer.Core;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace DigiMixer.UCNet.Core.Messages;

public class ParameterValueMessage : UCNetMessage
{
    public string Key { get; }
    // TODO: Rename
    public ushort Group { get; }
    public uint? Value { get; }

    public float? SingleValue
    {
        get
        {
            if (Value is not uint value)
            {
                return null;
            }
            // It's possible that using MemoryMarshal could simplify this a little.
            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
            return BinaryPrimitives.ReadSingleLittleEndian(bytes);
        }
    }

    public ParameterValueMessage(string key, ushort group, uint? value, MessageMode mode = MessageMode.FileRequest) : base(mode)
    {
        Key = key;
        Group = group;
        Value = value;
    }

    public ParameterValueMessage(string key, ushort group, float? value, MessageMode mode = MessageMode.FileRequest) : base(mode)
    {
        Key = key;
        Group = group;
        if (value is float v)
        {
            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteSingleLittleEndian(bytes, v);
            Value = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        }
    }

    protected override int BodyLength => Encoding.UTF8.GetByteCount(Key) + 1 + 2 + (Value is null ? 0 : 4);

    public override MessageType Type => MessageType.ParameterValue;

    protected override void WriteBody(Span<byte> span)
    {
        int textLength = Encoding.UTF8.GetBytes(Key, span);
        BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(textLength + 1), Group);
        if (Value is uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(textLength + 3), value);
        }
    }

    public static ParameterValueMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body)
    {
        int endOfKey = body.IndexOf((byte) 0);
        if (endOfKey == -1)
        {
            throw new ArgumentException("End of key not found in parameter value message.");
        }
        string key = Encoding.UTF8.GetString(body[..endOfKey]);
        ushort group = BinaryPrimitives.ReadUInt16LittleEndian(body.Slice(endOfKey + 1, 2));
        uint? value = body.Length == endOfKey + 7 ? BinaryPrimitives.ReadUInt32LittleEndian(body.Slice(endOfKey + 3, 4)) : null;
        return new ParameterValueMessage(key, group, value, mode);
    }

    public override string ToString() => $"ParameterValue: Key={Key}; Group={Group}; Value={Value}";
}
