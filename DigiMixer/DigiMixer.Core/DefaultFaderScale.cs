namespace DigiMixer.Core;

/// <summary>
/// A fader scale using the original fixed conversions.
/// (Over time, this should probably go away.)
/// </summary>
public sealed class DefaultFaderScale : IFaderScale
{
    public static DefaultFaderScale Instance { get; } = new();

    private DefaultFaderScale()
    {
    }

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
