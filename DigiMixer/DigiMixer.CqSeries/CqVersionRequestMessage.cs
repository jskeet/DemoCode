using DigiMixer.CqSeries.Core;

namespace DigiMixer.CqSeries;

internal class CqVersionRequestMessage : CqMessage
{
    public CqVersionRequestMessage() : base(CqMessageFormat.VariableLength, CqMessageType.VersionRequest, [])
    {
    }

    internal CqVersionRequestMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
