using System.Buffers.Binary;

namespace DigiMixer.UCNet.Core.Messages;

public class FileDataMessage(ushort id, uint totalSize, ushort offset, byte[] data, MessageMode mode = MessageMode.FileRequest)
    : UCNetMessage(mode)
{
    public ushort Id => id;
    public ushort Offset => offset;
    public uint TotalSize => totalSize;

    public ReadOnlySpan<byte> Data => data;

    public override MessageType Type => MessageType.FileData;

    protected override int BodyLength => data.Length + 14;

    protected override void WriteBody(Span<byte> span)
    {
        throw new NotImplementedException();
    }

    internal static FileDataMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body)
    {
        ushort id = BinaryPrimitives.ReadUInt16LittleEndian(body);
        ushort offset = BinaryPrimitives.ReadUInt16LittleEndian(body[2..]);
        ushort unknown1 = BinaryPrimitives.ReadUInt16LittleEndian(body[4..]);
        ushort totalSize = BinaryPrimitives.ReadUInt16LittleEndian(body[6..]);
        uint unknown2 = BinaryPrimitives.ReadUInt32LittleEndian(body[8..]);
        ushort chunkSize = BinaryPrimitives.ReadUInt16LittleEndian(body[12..]);

        if (body.Length != chunkSize + 14)
        {
            throw new InvalidDataException($"Invalid FileData message: chunk data size claimed to be {chunkSize} but packet body is {body.Length} bytes (with 14 byte chunk header)");
        }

        return new FileDataMessage(id, totalSize, offset, body[14..].ToArray(), mode);
    }

    public override string ToString() => $"FileData: ID={Id}; TotalSize={TotalSize}; Offset={Offset}; Length={data.Length}";
}

