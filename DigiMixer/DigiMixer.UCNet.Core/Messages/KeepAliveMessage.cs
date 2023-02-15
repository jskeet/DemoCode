namespace DigiMixer.UCNet.Core.Messages;

public class KeepAliveMessage : UCNetMessage
{
    public KeepAliveMessage(MessageMode mode = MessageMode.FileRequest) : base(mode)
    {
    }
    
    protected override int BodyLength => 0;
    public override MessageType Type => MessageType.KeepAlive;

    protected override void WriteBody(Span<byte> span)
    {
    }

    internal static KeepAliveMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body) =>
        new KeepAliveMessage(mode);

    public override string ToString() => $"KeepAlive";
}
