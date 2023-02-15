using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace DigiMixer.UCNet.Core;

internal static class Compression
{
    internal static byte[] ZLibCompress(byte[] uncompressedData)
    {
        using var outputStream = new MemoryStream();
        using var deflaterStream = new DeflaterOutputStream(outputStream);
        deflaterStream.Write(uncompressedData, 0, uncompressedData.Length);
        deflaterStream.Close();
        return outputStream.ToArray();
    }

    internal static byte[] ZLibDecompress(byte[] compressedData)
    {
        using var outputStream = new MemoryStream();
        using var inflaterStream = new InflaterInputStream(new MemoryStream(compressedData));
        inflaterStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }
}
