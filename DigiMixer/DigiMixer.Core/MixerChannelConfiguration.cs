namespace DigiMixer.Core;

/// <summary>
/// A configuration for <see cref="Mixer"/>, in terms of input/output channels.
/// This can either be constructed explicitly (e.g. for an application which is aware
/// of the desired channels) or detected by the underlying API code.
/// </summary>
public sealed class MixerChannelConfiguration(
    IEnumerable<ChannelId> inputChannels,
    IEnumerable<ChannelId> outputChannels,
    IEnumerable<StereoPair> stereoPairs)
{
    /// <summary>
    /// The input channels for the mixer.
    /// </summary>
    public IReadOnlyList<ChannelId> InputChannels { get; } = inputChannels.ToList().AsReadOnly();

    /// <summary>
    /// The output channels for the mixer.
    /// </summary>
    public IReadOnlyList<ChannelId> OutputChannels { get; } = outputChannels.ToList().AsReadOnly();

    /// <summary>
    /// The stereo pairings of input or output channels.
    /// </summary>
    public IReadOnlyList<StereoPair> StereoPairs { get; } = stereoPairs.ToList().AsReadOnly();
}
