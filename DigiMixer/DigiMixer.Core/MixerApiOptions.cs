namespace DigiMixer.Core;

/// <summary>
/// <remarks>
/// Mixer-neutral representation of options for how DigiMixer should
/// interface with any given mixer. These are inherently somewhat vague,
/// and all options may not be supported by all mixers.
/// </remarks>
/// </summary>
/// <remarks>
/// Concretely, the initial use case for this is to specify that a console
/// app used to control the mixer with an X-Touch Mini doesn't need meter updates.
/// </remarks>
public record MixerApiOptions
{
    public static MixerApiOptions Default { get; } = new MixerApiOptions();

    public MeterOptions MeterOptions { get; init; } = new MeterOptions();
}

public class MeterOptions
{
    public MeterUpdateFrequency UpdateFrequency { get; init; } = MeterUpdateFrequency.Fast;
}

/// <summary>
/// Generalized representation of how often we want to receive meter updates.
/// There is no specific meaning for "fast", "slow" etc - it's likely to be relative
/// within any given mixer implementation.
/// </summary>
public enum MeterUpdateFrequency
{
    Off,
    Slow,
    Medium,
    Fast
}