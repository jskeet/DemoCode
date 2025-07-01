using DigiMixer.AllenAndHeath.Core;

namespace DigiMixer.SqSeries;

public class SqClientInitResponseMessage : SqMessage
{
    // We don't know what this means at the moment, but it's always 1...
    public ushort MixerValue => GetUInt16(0);

    internal SqClientInitResponseMessage(AHRawMessage rawMessage) : base(rawMessage)
    {
    }

    public override string ToString() => $"Type={Type}; MixerValue={MixerValue}";
}
