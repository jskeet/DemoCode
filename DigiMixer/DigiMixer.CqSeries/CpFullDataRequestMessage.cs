namespace DigiMixer.CqSeries.Core;

public class CqFullDataRequestMessage : CqMessage
{
    public CqFullDataRequestMessage() : base(CqMessageFormat.VariableLength, CqMessageType.FullDataRequest, [])
    {
    }

    internal CqFullDataRequestMessage(CqRawMessage message) : base(message)
    {
    }
}
