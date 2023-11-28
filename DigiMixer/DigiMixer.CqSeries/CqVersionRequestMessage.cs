namespace DigiMixer.CqSeries.Core;

public class CqVersionRequestMessage : CqMessage
{
    public CqVersionRequestMessage() : base(CqMessageFormat.VariableLength, CqMessageType.VersionRequest, [])
    {
    }

    internal CqVersionRequestMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
