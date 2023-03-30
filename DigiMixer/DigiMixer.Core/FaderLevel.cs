namespace DigiMixer.Core;

/// <summary>
/// The level of a fader, within the scale of the mixer. This is represented
/// as a 10-bit integer; the mixer representation may vary, and the scale of what's represented
/// by each value may also vary significantly.
/// 
/// TODO: Make this much clearer, or dB-based
/// </summary>
public struct FaderLevel
{
    /// <summary>
    /// Maximum fader level, inclusive.
    /// </summary>
    /// <remarks>
    /// (This is an inclusive upper bound as
    /// often the value will be converted into a floating point value in the range 0-1, also
    /// inclusive.)
    /// </remarks>
    public const int MaxValue = 1024;

    /// <summary>
    /// The value of the fader level, in the range 0-1024 inclusive.
    /// </summary>
    public int Value { get; }

    public FaderLevel(int value) =>
        Value = value;

    public override string ToString() => $"Level: {Value}";
}
