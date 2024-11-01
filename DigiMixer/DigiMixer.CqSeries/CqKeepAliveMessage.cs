using DigiMixer.CqSeries.Core;

namespace DigiMixer.CqSeries;

public class CqKeepAliveMessage : CqMessage
{
    public CqKeepAliveMessage() : base(CqMessageFormat.VariableLength, CqMessageType.KeepAlive, [])
    {
    }

    internal CqKeepAliveMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
