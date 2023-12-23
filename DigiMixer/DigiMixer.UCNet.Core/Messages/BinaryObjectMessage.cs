namespace DigiMixer.UCNet.Core.Messages;

public class BinaryObjectMessage : UCNetMessage
{
    private readonly byte[] data;

    public BinaryObjectMessage(byte[] data, MessageMode mode = MessageMode.MixerUpdate) : base(mode)
    {
        this.data = data;
    }

    public override MessageType Type => MessageType.BinaryObject;

    protected override int BodyLength => data.Length;

    protected override void WriteBody(Span<byte> span) =>
        data.CopyTo(span);

    internal static BinaryObjectMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body) =>
        new BinaryObjectMessage(body.ToArray(), mode);

    public override string ToString() => $"BinaryObject: {data.Length} bytes";
}
