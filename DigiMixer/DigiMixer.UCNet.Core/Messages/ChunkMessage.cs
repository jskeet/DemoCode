namespace DigiMixer.UCNet.Core.Messages;

public class ChunkMessage : UCNetMessage
{
    private readonly byte[] data;

    public ReadOnlySpan<byte> Data => data;
    public uint Offset { get; }
    public uint TotalSize { get; }

    public ChunkMessage(uint totalSize, uint offset,  byte[] data, MessageMode mode) : base(mode)
    {
        TotalSize = totalSize;
        Offset = offset;
        this.data = data;
    }

    public override MessageType Type => MessageType.Chunk;

    protected override int BodyLength => throw new NotImplementedException();

    protected override void WriteBody(Span<byte> span)
    {
        throw new NotImplementedException();
    }

    internal static ChunkMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body)
    {
        uint offset = body.ReadUInt32();
        uint totalSize = body.ReadUInt32();
        uint chunkSize = body.ReadUInt32();
        if (chunkSize + 12 != body.Length)
        {
            throw new InvalidDataException($"Invalid Chunk message: chunk data size claimed to be {chunkSize} but message body is {body.Length} bytes (with 12 byte chunk header)");
        }
        return new ChunkMessage(totalSize, offset, body.Slice(12).ToArray(), mode);
    }

    public override string ToString() => $"Chunk: Offset={Offset}; Length={data.Length}; TotalSize={TotalSize}";
}
