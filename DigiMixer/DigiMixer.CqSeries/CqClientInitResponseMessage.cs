using DigiMixer.CqSeries.Core;

namespace DigiMixer.CqSeries;

public class CqClientInitResponseMessage : CqMessage
{
    // We don't know what this means at the moment, but it's always 1...
    public ushort MixerValue => GetUInt16(0);

    internal CqClientInitResponseMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
