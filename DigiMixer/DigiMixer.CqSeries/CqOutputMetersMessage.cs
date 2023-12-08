using DigiMixer.Core;
using DigiMixer.CqSeries.Core;

namespace DigiMixer.CqSeries;

internal class CqOutputMetersMessage : CqMessage
{
    internal CqOutputMetersMessage(CqRawMessage rawMessage) : base(rawMessage)
    {
    }

    internal MeterLevel GetLevelPostLimiter(int channelIndex)
    {
        var channelStart = channelIndex * 16;
        var raw = GetUInt16(channelStart + 14);
        return CqConversions.RawToMeterLevel(raw);
    }
}
