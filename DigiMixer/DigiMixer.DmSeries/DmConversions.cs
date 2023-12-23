using DigiMixer.Core;

namespace DigiMixer.DmSeries.Core;

internal static class DmConversions
{
    internal static DbFaderScale FaderScale { get; } = new(-121, -60, -50, -40, -35, -30, -25, -20, -15, -10, -7, -5, -2, 0, 2, 5, 7, 10);

    public static FaderLevel RawToFaderLevel(short raw) =>
        FaderScale.ConvertToFaderLevel(raw / 100.0d);

    public static short FaderLevelToRaw(FaderLevel level)
    {
        double db = FaderScale.ConvertToDb(level.Value);
        return db <= -121 ? short.MinValue : (short) (db * 100.0);
    }

    public static MeterLevel RawToMeterLevel(ushort raw)
    {
        var db = (raw - 0x8000) / 256.0;
        // TODO: Check this is still the case on the CQ!
        // The meters on the Qu-SB go up to 18dB; shift to have a 0dB limit like other mixers.
        db -= 18.0f;
        return MeterLevel.FromDb(db);
    }
}
