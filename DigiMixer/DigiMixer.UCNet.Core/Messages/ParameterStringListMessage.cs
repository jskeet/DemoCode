namespace DigiMixer.UCNet.Core.Messages;

public class ParameterStringListMessage : UCNetMessage
{
    private readonly byte[] data;

    public ParameterStringListMessage(byte[] data, MessageMode mode = MessageMode.FileRequest) : base(mode)
    {
        this.data = data;
    }

    public override MessageType Type => MessageType.ParameterStringList;

    protected override int BodyLength => data.Length;

    protected override void WriteBody(Span<byte> span)
    {
        throw new NotImplementedException();
    }

    internal static ParameterStringListMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body)
    {
        return new ParameterStringListMessage(body.ToArray(), mode);
    }

    public override string ToString() => $"ParameterStringList: FIXME";
}

