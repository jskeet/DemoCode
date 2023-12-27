namespace DigiMixer.Core;

/// <summary>
/// The level of a fader, within the scale of the mixer, as represented by its <see cref="IFaderScale"/>
/// implementation. This is simply an integer value; it only merits its own type to make it abundantly
/// clear that values are for fader levels, not meter levels (or arbitrary protocol values).
/// </summary>
public struct FaderLevel
{
    /// <summary>
    /// The value of the fader level, in the range indicated by the corresponding fader scale.
    /// </summary>
    public int Value { get; }

    public FaderLevel(int value) => Value = value;

    public override string ToString() => $"Level: {Value}";
}
