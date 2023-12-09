using DigiMixer.DmSeries.Core;

namespace DigiMixer.DmSeries;

public sealed class DmUnknownMessage : DmMessage
{
    public DmUnknownMessage(DmRawMessage rawMessage) : base(rawMessage)
    {
    }
}
