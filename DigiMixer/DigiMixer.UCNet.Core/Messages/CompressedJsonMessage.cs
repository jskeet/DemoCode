using System.Buffers.Binary;

namespace DigiMixer.UCNet.Core.Messages;

public class CompressedJsonMessage : UCNetMessage
{

    private readonly byte[] compressedData;
    private readonly byte[] uncompressedData;

    public ReadOnlySpan<byte> CompressedData => compressedData;
    public ReadOnlySpan<byte> UncompressedData => uncompressedData;

    private CompressedJsonMessage(byte[] compressedData, byte[] uncompressedData, MessageMode mode) : base(mode)
    {
        this.compressedData = compressedData;
        this.uncompressedData = uncompressedData;
    }

    public override MessageType Type => MessageType.CompressedJson;
    protected override int BodyLength => compressedData.Length + 4;

    public static CompressedJsonMessage FromCompressedData(byte[] compressedData, MessageMode mode = MessageMode.Compressed)
    {
        var uncompressedData = Compression.ZLibDecompress(compressedData);
        return new CompressedJsonMessage(compressedData, uncompressedData, mode);
    }

    public static CompressedJsonMessage FromUncompressedData(byte[] uncompressedData, MessageMode mode = MessageMode.Compressed)
    {
        var compressedData = Compression.ZLibCompress(uncompressedData);
        return new CompressedJsonMessage(compressedData, uncompressedData, mode);
    }

    protected override void WriteBody(Span<byte> span)
    {
        BinaryPrimitives.WriteInt32LittleEndian(span, compressedData.Length);
        compressedData.CopyTo(span.Slice(4));
    }

    internal static CompressedJsonMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body)
    {
        int length = BinaryPrimitives.ReadInt32LittleEndian(body);
        if (length != body.Length - 4)
        {
            throw new ArgumentException($"Message starts claiming compressed data length {length} but is {body.Length}");
        }
        return FromCompressedData(body.Slice(4).ToArray(), mode);
    }

    public string ToJson() => Ubjson.ToJson(uncompressedData);

    public override string ToString() => $"CompressedJson: {uncompressedData.Length} bytes (compressed to {compressedData.Length})";
}
