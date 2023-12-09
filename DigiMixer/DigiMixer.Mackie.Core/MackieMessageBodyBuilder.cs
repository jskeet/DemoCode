using DigiMixer.Core;

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
        BigEndian.WriteUInt32(data.AsSpan().Slice(chunk * 4), value);
        return this;
    }

    // TODO: Support 
    public MackieMessageBodyBuilder SetSingle(int chunk, float value) =>
        SetInt32(chunk, BitConverter.SingleToInt32Bits(value));

    public MackieMessageBodyBuilder SetInt32(int chunk, int value) =>
        SetUInt32(chunk, (uint) value);

    public MackieMessageBody Build() => new MackieMessageBody(data);
}
