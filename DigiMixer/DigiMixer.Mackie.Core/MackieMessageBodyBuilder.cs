using System.Buffers.Binary;

namespace DigiMixer.Mackie.Core;

public sealed class MackieMessageBodyBuilder(int chunks)
{
    private readonly byte[] data = new byte[chunks * 4];

    public MackieMessageBodyBuilder SetUInt32(int chunk, uint value)
    {
        BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan()[(chunk * 4)..], value);
        return this;
    }

    public MackieMessageBodyBuilder SetSingle(int chunk, float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(data.AsSpan()[(chunk * 4)..], value);
        return this;
    }

    public MackieMessageBodyBuilder SetInt32(int chunk, int value) =>
        SetUInt32(chunk, (uint) value);

    public MackieMessageBody Build() => new(data);
}
