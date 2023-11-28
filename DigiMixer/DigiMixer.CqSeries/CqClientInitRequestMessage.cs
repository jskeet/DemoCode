namespace DigiMixer.CqSeries.Core;

public class CqClientInitRequestMessage : CqMessage
{
    public CqClientInitRequestMessage() : base(CqMessageFormat.VariableLength, CqMessageType.ClientInitRequest, [2, 0])
    {
    }

    internal CqClientInitRequestMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
