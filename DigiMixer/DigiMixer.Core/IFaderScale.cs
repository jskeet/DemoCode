namespace DigiMixer.Core;

/// <summary>
/// The scale used by a particular <see cref="IMixerApi"/> to represent fader levels,
/// both to set a maximum value and to convert fader levels to decibel values.
/// The mixer API should provide and accept values such that a linear movement of a physical
/// fader naturally maps to a linear movement of fader values.
/// The minimum value is assumed to be 0, which should always be converted to "-infinity" decibels.
/// No assumptions are made about the maximum decibel range of faders.
/// All faders on a single mixer are assumed to use the same scale.
/// </summary>
public interface IFaderScale
{
    /// <summary>
    /// The maximum underlying integer value to be represented by a <see cref="FaderLevel"/> for
    /// this mixer.
    /// </summary>
    int MaxValue { get; }

    /// <summary>
    /// Converts the given underlying integer level to a decibel level.
    /// </summary>
    double ConvertToDb(int level);
}
