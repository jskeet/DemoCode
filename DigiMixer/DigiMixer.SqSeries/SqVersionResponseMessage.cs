using DigiMixer.AllenAndHeath.Core;

namespace DigiMixer.SqSeries;

internal class SqVersionResponseMessage : SqMessage
{
    public string Version => $"{Data[1]}.{Data[2]}.{Data[3]} r{GetUInt16(4)}";

    public SqVersionResponseMessage(byte[] data) : base(SqMessageType.VersionResponse, data)
    {
    }

    internal SqVersionResponseMessage(AHRawMessage rawMessage) : base(rawMessage)
    {
    }

    public override string ToString() => $"Type={Type}; Version={Version}";
}
