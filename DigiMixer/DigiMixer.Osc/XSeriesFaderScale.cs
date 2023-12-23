using DigiMixer.Core;

namespace DigiMixer.Osc;

/// <summary>
/// Hand-crafted fader scale for Behringer X-Air; may not be suitable for RCF.
/// </summary>
internal sealed class XSeriesFaderScale : IFaderScale
{
    public int MaxValue => 1024;

    public double ConvertToDb(int level) =>
        (level / (float) MaxValue) switch
        {
            float f when f >= 0.5f => f * 40 - 30,
            float f when f >= 0.25f => f * 80 - 50,
            float f when f >= 0.0625f => f * 160 - 70,
            float f when f == 0f => double.NegativeInfinity,
            float f => f * 480 - 90
        };
}
