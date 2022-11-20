using DigiMixer.Core;

namespace DigiMixer.Mackie;

/// <summary>
/// All conversions for faders and meter levels.
/// </summary>
internal static class MackieConversions
{
    internal static FaderLevel ToFaderLevel(float db)
    {
        // Evenly spaced:
        // -120 (well a little bit nearer)
        // -60
        // -40
        // -30
        // -20
        // -10
        // 0
        // 5
        // 10

        var spacing = FaderLevel.MaxValue / 8;
        int value = db switch
        {
            >= 10 => FaderLevel.MaxValue,
            >= 5 => (int) ((db - 5) * (spacing / 5f) + spacing * 7),
            >= 0 => (int) ((db - 0) * (spacing / 5f) + spacing * 6),
            >= -10 => (int) ((db + 10) * (spacing / 10f) + spacing * 5),
            >= -20 => (int) ((db + 20) * (spacing / 10f) + spacing * 4),
            >= -30 => (int) ((db + 30) * (spacing / 10f) + spacing * 3),
            >= -40 => (int) ((db + 40) * (spacing / 10f) + spacing * 2),
            >= -60 => (int) ((db + 60) * (spacing / 20f) + spacing * 1),
            >= -120 => (int) ((db + 120) * (spacing / 60f) + spacing * 0),
            _ => 0
        };
        return new FaderLevel(value);
    }

    internal static float FromFaderLevel(FaderLevel level)
    {
        const int spacing = FaderLevel.MaxValue / 8;
        int space = level.Value / spacing;
        float withinSpace = (level.Value / (float) spacing) - space; // [0, 1)
        return space switch
        {
            < 0 => -120,
            0 => -120f + 60f * withinSpace,
            1 => -60 + 20f * withinSpace,
            2 => -40 + 10f * withinSpace,
            3 => -30 + 10f * withinSpace,
            4 => -20 + 10f * withinSpace,
            5 => -10 + 10f * withinSpace,
            6 => 0f + 5f * withinSpace,
            7 => 5f + 5f * withinSpace,
            >= 8 => 10f
        };
    }

    internal static MeterLevel ToMeterLevel(float value) => MeterLevel.FromDb(value);
}
