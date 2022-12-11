namespace DigiMixer.Mackie.Core;

public sealed class MackiePacketBodyBuilder
{
    private byte[] data;

    public MackiePacketBodyBuilder(int chunks)
    {
        data = new byte[chunks * 4];
    }

    public MackiePacketBodyBuilder SetUInt32(int chunk, uint value)
    {
        int offset = chunk * 4;
        data[offset] = (byte) (value >> 24);
        data[offset + 1] = (byte) (value >> 16);
        data[offset + 2] = (byte) (value >> 8);
        data[offset + 3] = (byte) (value >> 0);
        return this;
    }

    public MackiePacketBodyBuilder SetSingle(int chunk, float value) =>
        SetInt32(chunk, BitConverter.SingleToInt32Bits(value));

    public MackiePacketBodyBuilder SetInt32(int chunk, int value) =>
        SetUInt32(chunk, (uint) value);

    public MackiePacketBody Build() => new MackiePacketBody(data);
}
