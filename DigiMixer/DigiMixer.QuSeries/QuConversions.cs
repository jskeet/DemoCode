using DigiMixer.Core;

namespace DigiMixer.QuSeries;

internal class QuConversions
{
    internal static DbFaderScale FaderScale = new(-128, -40, -30, -20, -10, -5, 0, 4, 10);

    internal static FaderLevel RawToFaderLevel(ushort raw)
    {
        float db = (raw - 0x8000) / 256.0f;
        return FaderScale.ConvertToFaderLevel(db);
    }

    internal static ushort FaderLevelToRaw(FaderLevel level)
    {
        if (level.Value <= 0)
        {
            return 0;
        }
        double db = FaderScale.ConvertToDb(level.Value);
        return (ushort) ((db * 256.0) + 0x8000);
    }

    public static MeterLevel RawToMeterLevel(ushort raw)
    {
        var db = (raw - 0x8000) / 256.0;
        // The meters on the Qu-SB go up to 18dB; shift to have a 0dB limit like other mixers.
        db -= 18.0f;
        return MeterLevel.FromDb(db);
    }

    internal static ChannelId? NetworkToChannelId(int channel) => channel switch
    {
        >= 0 and < 32 => ChannelId.Input(channel + 1),
        // Mono mixes
        >= 39 and <= 42 => ChannelId.Output(channel - 38),
        // Stereo mixes (left channel)
        >= 43 and <= 45 => ChannelId.Output((channel - 43) * 2 + 5),
        46 => ChannelId.MainOutputLeft,
        // Stereo groups (left channel)
        >= 47 and <= 50 => GroupChannelId((channel - 47) * 2 + 1),
        _ => null
    };

    internal static int ChannelIdToNetwork(ChannelId channel) =>
        channel.IsInput ? channel.Value - 1
        : channel.IsMainOutput ? 46
        // Group outputs (47-50)
        : channel.Value >= 21 && channel.Value <= 28 ? (channel.Value - 21) / 2 + 47
        // Mix outputs
        : channel.Value + 38;

    // Special channel IDs:
    // Output 21-28: Groups
    // Each stereo group has a separate channel, but 1-2, 3-4, 5-6, 7-8 are bonded.
    public static ChannelId GroupChannelId(int group) => ChannelId.Output(group + 20);

}
