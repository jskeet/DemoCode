using DigiMixer.CqSeries.Core;

namespace DigiMixer.CqSeries;

internal class CqVersionResponseMessage : CqMessage
{
    public string Version => $"{Data[1]}.{Data[2]}.{Data[3]} r{GetUInt16(4)}";

    public CqVersionResponseMessage(byte[] data) : base(CqMessageFormat.VariableLength, CqMessageType.VersionResponse, data)
    {
    }

    internal CqVersionResponseMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
