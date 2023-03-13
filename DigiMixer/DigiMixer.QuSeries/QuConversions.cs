using DigiMixer.Core;

namespace DigiMixer.QuSeries;

internal class QuConversions
{
    private const long MaxRawFaderLevel = 0x8a00;

    internal static FaderLevel RawToFaderLevel(ushort raw) =>
        new FaderLevel((int) ((long) raw * FaderLevel.MaxValue / MaxRawFaderLevel));

    internal static ushort FaderLevelToRaw(FaderLevel level) =>
        (ushort) (level.Value * MaxRawFaderLevel / FaderLevel.MaxValue);

    public static MeterLevel RawToMeterLevel(ushort raw)
    {
        var db = (raw - 0x8000) / 256.0;
        return MeterLevel.FromDb(db);
    }

    internal static ChannelId? NetworkToChannelId(int channel) => channel switch
    {
        >= 0 and < 32 => ChannelId.Input(channel + 1),
        >= 39 and <= 45 => ChannelId.Output(channel - 38),
        46 => ChannelId.MainOutputLeft,
        _ => null
    };

    internal static int ChannelIdToNetwork(ChannelId channel) =>
        channel.IsInput ? channel.Value - 1
        : channel.IsMainOutput ? 46
        : channel.Value + 38;
}
