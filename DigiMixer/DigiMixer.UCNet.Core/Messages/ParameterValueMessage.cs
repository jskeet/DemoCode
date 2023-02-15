using System.Text;

namespace DigiMixer.UCNet.Core.Messages;

public class ParameterValueMessage : UCNetMessage
{
    public string Key { get; }
    // TODO: Rename
    public ushort Group { get; }
    public uint? Value { get; }

    public ParameterValueMessage(string key, ushort group, uint? value, MessageMode mode = MessageMode.FileRequest) : base(mode)
    {
        Key = key;
        Group = group;
        Value = value;
    }

    protected override int BodyLength => Encoding.UTF8.GetByteCount(Key) + 1 + 2 + (Value is null ? 0 : 4);

    public override MessageType Type => MessageType.ParameterValue;

    protected override void WriteBody(Span<byte> span)
    {
        int textLength = span.WriteString(Key);
        span.Slice(textLength + 1).WriteUInt16(Group);
        if (Value is uint value)
        {
            span.Slice(textLength + 3).WriteUInt32(value);
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
        ushort group = body.Slice(endOfKey + 1, 2).ReadUInt16();
        uint? value = body.Length == endOfKey + 7 ? body.Slice(endOfKey + 3, 4).ReadUInt32() : null;
        return new ParameterValueMessage(key, group, value, mode);
    }

    public override string ToString() => $"ParameterValue: Key={Key}; Group={Group}; Value={Value}";
}
