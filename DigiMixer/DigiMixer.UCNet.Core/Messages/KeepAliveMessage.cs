namespace DigiMixer.UCNet.Core.Messages;

public class KeepAliveMessage(MessageMode mode = MessageMode.FileRequest) : UCNetMessage(mode)
{
    protected override int BodyLength => 0;
    public override MessageType Type => MessageType.KeepAlive;

    protected override void WriteBody(Span<byte> span)
    {
    }

    // TODO: validate that the body is empty, maybe?
#pragma warning disable IDE0060 // Remove unused parameter
    internal static KeepAliveMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body) =>
        new(mode);
#pragma warning restore IDE0060 // Remove unused parameter

    public override string ToString() => "KeepAlive";
}
