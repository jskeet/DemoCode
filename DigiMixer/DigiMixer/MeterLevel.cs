namespace DigiMixer;

/// <summary>
/// The level of a meter, e.g. the current output of a channel,
/// with a maximum level of 0dB.
/// </summary>
public struct MeterLevel
{
    public double Value { get; }

    // TODO: ToString, linearize to a given integer scale.
    public MeterLevel(double value) =>
        Value = value;
}
