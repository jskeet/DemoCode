using DigiMixer.Core;

namespace DigiMixer.CqSeries.Core;

public class CqRegularMessage : CqMessage
{
    public override CqMessageType Type => CqMessageType.Regular;

    public byte X => Data[0];
    public byte Y => Data[1];
    public byte Seq => Data[2];

    public CqRegularMessage(CqMessageFormat format, byte x, byte y, byte seq, byte[] remainingData) : base(format, new[] { x, y, seq }.Concat(remainingData).ToArray())
    {
    }

    internal CqRegularMessage(CqMessageFormat format, byte[] data) : base(format, data)
    {
    }

    public override string ToString() => Data.Length > 9
        ? $"Type={Type}; X={X}; Y={Y}; Seq={Seq}; TotalDataLength={Data.Length}"
        : $"Type={Type}; X={X}; Y={Y}; Seq={Seq}; OtherData={Formatting.ToHex(Data.Slice(3))}";
}
