using System.Text;

namespace DigiMixer.UCNet.Core.Messages;

public class FileRequestMessage : UCNetMessage
{
    private readonly string path;
    // The size of the path in bytes.
    private readonly int byteCount;

    private readonly ushort requestId;

    public FileRequestMessage(string path, ushort requestId, MessageMode mode = MessageMode.FileRequest) : base(mode)
    {
        this.path = path;
        this.requestId = requestId;
        byteCount = Encoding.UTF8.GetByteCount(path);
    }

    // 2 bytes for 0x01 0x00 before, then 0x00 0x00 at the end
    protected override int BodyLength => byteCount + 4;

    public override MessageType Type => MessageType.FileRequest;

    protected override void WriteBody(Span<byte> span)
    {
        span.WriteUInt16(requestId);
        span.Slice(2).WriteString(path);
    }

    internal static FileRequestMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body)
    {
        var requestId = body.ReadUInt16();
        var textSpan = body[2..^2];
        return new FileRequestMessage(textSpan.ReadString(), requestId, mode);
    }

    public override string ToString() => $"FileRequest: Path={path}";
}
