namespace DigiMixer.UCNet.Core.Messages;

public class BinaryObjectMessage(byte[] data, MessageMode mode = MessageMode.MixerUpdate) : UCNetMessage(mode)
{
    public override MessageType Type => MessageType.BinaryObject;

    protected override int BodyLength => data.Length;

    protected override void WriteBody(Span<byte> span) =>
        data.CopyTo(span);

    internal static BinaryObjectMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body) =>
        new(body.ToArray(), mode);

    public override string ToString() => $"BinaryObject: {data.Length} bytes";
}
