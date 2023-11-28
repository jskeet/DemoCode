namespace DigiMixer.CqSeries.Core;

public class CqClientInitResponseMessage : CqMessage
{
    // We don't know what this means at the moment, but it's always 1...
    public ushort MixerValue => GetUInt16(0);

    internal CqClientInitResponseMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }
}
