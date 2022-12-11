namespace DigiMixer.Core;

/// <summary>
/// Describes how a <see cref="StereoPair"/> behaves in the mixer.
/// This can affect how it should be treated by other code; for example, if
/// <see cref="SplitMutes"/> is included, then clients should set the mute status for both channels
/// at the same time.
/// </summary>
[Flags]
public enum StereoFlags
{
    /// <summary>
    /// The stereo channel behaves as a single channel, with one name, one mute, one fader.
    /// </summary>
    None = 0,

    /// <summary>
    /// The stereo channel has two independent names.
    /// </summary>
    SplitNames = 1 << 0,

    /// <summary>
    /// The stereo channel has two independent mutes.
    /// </summary>
    SplitMutes = 1 << 1,

    /// <summary>
    /// The stereo channel has two independent faders.
    /// </summary>
    SplitFaders = 1 << 2,

    /// <summary>
    /// Every aspect of the stereo channel is split.
    /// </summary>
    FullyIndependent = SplitNames | SplitMutes | SplitFaders
}