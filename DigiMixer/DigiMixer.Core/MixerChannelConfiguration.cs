using System.Collections.Immutable;

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
    public ImmutableArray<ChannelId> InputChannels { get; } = [.. inputChannels];

    /// <summary>
    /// The output channels for the mixer.
    /// </summary>
    public ImmutableArray<ChannelId> OutputChannels { get; } = [.. outputChannels];

    /// <summary>
    /// The stereo pairings of input or output channels.
    /// </summary>
    public ImmutableArray<StereoPair> StereoPairs { get; } = [.. stereoPairs];
}
