using DigiMixer.CqSeries.Core;

namespace DigiMixer.CqSeries;

internal sealed class CqUnknownMessage : CqMessage
{
    internal CqUnknownMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
