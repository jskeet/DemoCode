using DigiMixer.Core;

namespace DigiMixer.CqSeries.Core;

public class CqConversions
{
    internal static DbFaderScale FaderScale { get; } = new(-100, -40, -30, -20, -10, -5, -1, 5, 10);

    public static FaderLevel RawToFaderLevel(ushort raw) =>
        FaderScale.ConvertToFaderLevel((raw - 0x8000) / 256.0d);

    public static ushort FaderLevelToRaw(FaderLevel level)
    {
        double db = FaderScale.ConvertToDb(level.Value);
        return db <= -120 ? (ushort) 0 : (ushort) ((db * 256.0) + 0x8000);
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
