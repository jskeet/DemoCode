using System.Buffers.Binary;

namespace DigiMixer.UCNet.Core.Messages;

public class ChunkMessage(uint totalSize, uint offset, byte[] data, MessageMode mode) : UCNetMessage(mode)
{
    public ReadOnlySpan<byte> Data => data;
    public uint Offset => offset;
    public uint TotalSize => totalSize;

    public override MessageType Type => MessageType.Chunk;

    protected override int BodyLength => throw new NotImplementedException();

    protected override void WriteBody(Span<byte> span)
    {
        throw new NotImplementedException();
    }

    internal static ChunkMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body)
    {
        uint offset = BinaryPrimitives.ReadUInt32LittleEndian(body);
        uint totalSize = BinaryPrimitives.ReadUInt32LittleEndian(body[4..]);
        uint chunkSize = BinaryPrimitives.ReadUInt32LittleEndian(body[8..]);
        if (chunkSize + 12 != body.Length)
        {
            throw new InvalidDataException($"Invalid Chunk message: chunk data size claimed to be {chunkSize} but message body is {body.Length} bytes (with 12 byte chunk header)");
        }
        return new ChunkMessage(totalSize, offset, body[12..].ToArray(), mode);
    }

    public override string ToString() => $"Chunk: Offset={Offset}; Length={data.Length}; TotalSize={TotalSize}";
}
