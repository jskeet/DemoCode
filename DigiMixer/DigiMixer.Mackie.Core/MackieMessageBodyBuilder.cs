using DigiMixer.Core;
using System.Buffers.Binary;

namespace DigiMixer.Mackie.Core;

public sealed class MackieMessageBodyBuilder
{
    private byte[] data;

    public MackieMessageBodyBuilder(int chunks)
    {
        data = new byte[chunks * 4];
    }

    public MackieMessageBodyBuilder SetUInt32(int chunk, uint value)
    {
        BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan().Slice(chunk * 4), value);
        return this;
    }

    public MackieMessageBodyBuilder SetSingle(int chunk, float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(data.AsSpan().Slice(chunk * 4), value);
        return this;
    }

    public MackieMessageBodyBuilder SetInt32(int chunk, int value) =>
        SetUInt32(chunk, (uint) value);

    public MackieMessageBody Build() => new MackieMessageBody(data);
}
