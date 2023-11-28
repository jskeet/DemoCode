namespace DigiMixer.CqSeries.Core;

public class CqKeepAliveMessage : CqMessage
{
    public CqKeepAliveMessage() : base(CqMessageFormat.VariableLength, CqMessageType.KeepAlive, [])
    {
    }

    internal CqKeepAliveMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
