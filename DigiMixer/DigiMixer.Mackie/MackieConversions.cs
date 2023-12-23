using DigiMixer.Core;

namespace DigiMixer.Mackie;

/// <summary>
/// All conversions for faders and meter levels.
/// </summary>
internal static class MackieConversions
{
    internal static DbFaderScale FaderScale { get; } = new DbFaderScale(-120, -60, -40, -30, -20, -10, -5, 0, 5, 10);

    internal static FaderLevel ToFaderLevel(float db) =>
        FaderScale.ConvertToFaderLevel(db);

    // Clamp to a minimum of -120
    internal static float FromFaderLevel(FaderLevel level) =>
        (float) Math.Max(FaderScale.ConvertToDb(level.Value), -120d);

    internal static MeterLevel ToMeterLevel(float value) => MeterLevel.FromDb(value);
}
