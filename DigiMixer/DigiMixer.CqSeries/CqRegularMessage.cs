using DigiMixer.Core;

namespace DigiMixer.CqSeries.Core;

public class CqRegularMessage : CqMessage
{
    public static (byte X, byte Y, byte Z) SetFaderXyz => (6, 6, 14);
    public static (byte X, byte Y, byte Z) SetMuteXyz => (6, 6, 12);
    public static (byte X, byte Y, byte Z) UnitInfoRequestXyz => (0x1a, 0x1a, 0x0d);
    public static (byte X, byte Y, byte Z) UnitInfoResponseXyz => (0x1a, 0x1a, 0x0e);
    public static (byte X, byte Y, byte Z) SetNameXyz => (35, 0, 10);

    public byte X => Data[0];
    public byte Y => Data[1];
    public byte Z => Data[2];

    public (byte X, byte Y, byte Z) XYZ => (X, Y, Z);

    public CqRegularMessage(CqMessageFormat format, byte x, byte y, byte z, byte[] remainingData) : this(format, (x, y, z), remainingData)
    {
    }

    public CqRegularMessage((byte X, byte Y, byte Z) xyz, byte[] remainingData) : this(CqMessageFormat.FixedLength8, xyz, remainingData)
    {
    }

    public CqRegularMessage((byte X, byte Y, byte Z) xyz, byte sub1, byte sub2, ushort value) :
        base(CqMessageFormat.FixedLength8, CqMessageType.Regular, [xyz.X, xyz.Y, xyz.Z, sub1, sub2, (byte) value, (byte) (value >> 8)])
    {
    }

    public CqRegularMessage(CqMessageFormat format, (byte X, byte Y, byte Z) xyz, byte[] remainingData) : base(format, CqMessageType.Regular, new[] { xyz.X, xyz.Y, xyz.Z }.Concat(remainingData).ToArray())
    {
    }

    internal CqRegularMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }

    public override string ToString() => Data.Length > 16
        ? $"Type={Type}; X={X}; Y={Y}; Z={Z}; TotalDataLength={Data.Length}"
        : $"Type={Type}; X={X}; Y={Y}; Z={Z}; OtherData={Formatting.ToHex(Data.Slice(3))}";
}
