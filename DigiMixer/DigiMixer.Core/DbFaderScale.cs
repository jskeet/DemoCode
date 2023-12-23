namespace DigiMixer.Core;

/// <summary>
/// An implementation of <see cref="IFaderScale"/> for mixers with protocols
/// which naturally provide dB levels instead of linear levels. The linear
/// <see cref="FaderLevel"/> scale is provided by interpolating between fixed
/// (mixer-specific) points.
/// </summary>
public sealed class DbFaderScale : IFaderScale
{
    /// <summary>
    /// The number of integer values between each dB value provided in the constructor.
    /// </summary>
    private const int ValuesPerGap = 1024;

    private readonly double[] dbValues;

    public int MaxValue { get; }

    /// <summary>
    /// Constructs a scale based on the given dB values, to be equally spaced in the integer range.
    /// </summary>
    /// <param name="dbValues">The dB values which should be equally spaced in the fader.
    /// The first value should be the effective "anything this or lower is -infinity",
    /// and the last value should be the maximum value supported by the mixer.</param>
    public DbFaderScale(params double[] dbValues)
    {
        MaxValue = (dbValues.Length - 1) * ValuesPerGap;
        this.dbValues = (double[]) dbValues.Clone();
    }

    public FaderLevel ConvertToFaderLevel(double db)
    {
        if (db < dbValues[0])
        {
            return new FaderLevel(0);
        }
        int index = Array.BinarySearch(dbValues, db);
        if (index < 0)
        {
            index = (~index) - 1;
        }
        if (index >= dbValues.Length - 1)
        {
            return new FaderLevel(MaxValue);
        }
        double lowerBound = dbValues[index];
        double upperBound = dbValues[index + 1];
        var spaceSize = upperBound - lowerBound;
        var valueWithinSpace = db - lowerBound;
        int indexWithinSpace = (int) ((valueWithinSpace / spaceSize) * ValuesPerGap);
        return new FaderLevel(index * ValuesPerGap + indexWithinSpace);
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
        int index = level / ValuesPerGap;
        double lowerBound = dbValues[index];
        double upperBound = dbValues[index + 1];
        int offset = level % ValuesPerGap;
        return lowerBound + ((upperBound - lowerBound) * offset) / ValuesPerGap;
    }
}
