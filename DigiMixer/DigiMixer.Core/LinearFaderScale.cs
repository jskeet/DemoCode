namespace DigiMixer.Core;

/// <summary>
/// An implementation of <see cref="IFaderScale"/> designed for mixer protocols which already have
/// a linear relationship between the value in the protocol and the logical position of the fader,
/// so only need to map that to decibels.
/// </summary>
public sealed class LinearFaderScale : IFaderScale
{
    public int MaxValue { get; }

    private readonly int[] rawValues;
    private readonly double[] dbValues;

    /// <summary>
    /// Creates a linear scale from the given set of reference points.
    /// The first reference point should have a value of 1 and a dB value representing
    /// the lowest non-negative-infinity decibel value that can be handled by the mixer.
    /// The final reference point should represent the maximum dB value that can be handled
    /// by the mixer (on a fader) and the corresponding value provides the maximum value for the scale.
    /// </summary>
    /// <param name="referencePoints"></param>
    public LinearFaderScale(params (int value, double db)[] referencePoints)
    {
        rawValues = referencePoints.Select(rp => rp.value).ToArray();
        dbValues = referencePoints.Select(rp => rp.db).ToArray();
        MaxValue = rawValues.Last();
    }

    public double ConvertToDb(int level)
    {
        if (level >= MaxValue)
        {
            return dbValues[dbValues.Length - 1];
        }
        if (level <= 0)
        {
            return double.NegativeInfinity;
        }
        int index = Array.BinarySearch(rawValues, level);
        if (index < 0)
        {
            index = (~index) - 1;
        }
        // This shouldn't happen anyway, but let's keep safe.
        if (index >= dbValues.Length)
        {
            return dbValues[dbValues.Length - 1];
        }
        double lowerBoundDb = dbValues[index];
        double upperBoundDb = dbValues[index + 1];
        int lowerBoundValue = rawValues[index];
        int upperBoundValue = rawValues[index + 1];
        var spaceSize = upperBoundValue - lowerBoundValue;
        var indexWithinSpace = level - lowerBoundValue;
        return (upperBoundDb - lowerBoundDb) * indexWithinSpace / spaceSize + lowerBoundDb;
    }
}
