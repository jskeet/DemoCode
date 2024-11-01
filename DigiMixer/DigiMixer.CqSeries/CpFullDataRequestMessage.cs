using DigiMixer.CqSeries.Core;

namespace DigiMixer.CqSeries;

internal class CqFullDataRequestMessage : CqMessage
{
    internal CqFullDataRequestMessage() : base(CqMessageFormat.VariableLength, CqMessageType.FullDataRequest, [])
    {
    }

    internal CqFullDataRequestMessage(CqRawMessage message) : base(message)
    {
    }
}
