using DigiMixer.Core;
using DigiMixer.CqSeries.Core;

namespace DigiMixer.CqSeries;

internal class CqInputMetersMessage : CqMessage
{
    internal CqInputMetersMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }

    internal MeterLevel GetLevelPostComp(int channelIndex)
    {
        var channelStart = channelIndex * 20;
        var raw = GetUInt16(channelStart + 14);
        return CqConversions.RawToMeterLevel(raw);
    }
}
