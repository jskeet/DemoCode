namespace DigiMixer.CqSeries.Core;

public sealed class CqUnknownMessage : CqMessage
{
    public CqUnknownMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
