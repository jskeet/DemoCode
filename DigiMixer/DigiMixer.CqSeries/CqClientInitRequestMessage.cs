using DigiMixer.CqSeries.Core;

namespace DigiMixer.CqSeries;

internal class CqClientInitRequestMessage : CqMessage
{
    internal CqClientInitRequestMessage() : base(CqMessageFormat.VariableLength, CqMessageType.ClientInitRequest, [2, 0])
    {
    }

    internal CqClientInitRequestMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
