namespace DigiMixer.UCNet.Core.Messages;

public class Meter8Message : MeterMessageBase<byte>
{
    private Meter8Message(string meterType, byte[] data, byte[] rowMappingData, MessageMode mode)
    : base(meterType, data, rowMappingData, mode)
    {
    }

    public override MessageType Type => MessageType.Meter16;

    protected override byte ReadValue(int index) => Data[index];
    protected override int ValueSize => 1;

    internal static Meter8Message FromRawBody(MessageMode mode, ReadOnlySpan<byte> body)
    {
        string type = body.Slice(0, 4).ReadString();
        int dataCount = body.Slice(6, 2).ReadUInt16();
        var data = body.Slice(8, dataCount);
        int rowCount = body.Slice(8 + data.Length)[0];
        var rowMappingData = body.Slice(8 + data.Length + 1, rowCount * 6);
        if (rowMappingData.Length + data.Length + 9 != body.Length)
        {
            throw new ArgumentException($"Unexpected length; was {body.Length} bytes for {dataCount} values and {rowCount} row maps");
        }
        return new Meter8Message(type, data.ToArray(), rowMappingData.ToArray(), mode);
    }
}
