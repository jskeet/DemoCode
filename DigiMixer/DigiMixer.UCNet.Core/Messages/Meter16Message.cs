using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.UCNet.Core.Messages;

public class Meter16Message : MeterMessageBase<uint>
{
    private Meter16Message(string meterType, byte[] data, byte[] rowMappingData, MessageMode mode)
        : base(meterType, data, rowMappingData, mode)
    {
    }

    public override MessageType Type => MessageType.Meter16;

    protected override uint ReadValue(int index) => BinaryPrimitives.ReadUInt16LittleEndian(Data.Slice(index * 2));
    protected override int ValueSize => 2;

    internal static Meter16Message FromRawBody(MessageMode mode, ReadOnlySpan<byte> body)
    {
        string type = Encoding.UTF8.GetString(body.Slice(0, 4));
        int dataCount = BinaryPrimitives.ReadUInt16LittleEndian(body.Slice(6, 2));
        var data = body.Slice(8, dataCount * 2);
        int rowCount = body.Slice(8 + data.Length)[0];
        var rowMappingData = body.Slice(8 + data.Length + 1, rowCount * 6);
        if (rowMappingData.Length + data.Length + 9 != body.Length)
        {
            throw new ArgumentException($"Unexpected length; was {body.Length} bytes for {dataCount} values and {rowCount} row maps");
        }
        return new Meter16Message(type, data.ToArray(), rowMappingData.ToArray(), mode);
    }
}
