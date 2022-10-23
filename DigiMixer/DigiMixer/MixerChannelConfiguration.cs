namespace DigiMixer;

/// <summary>
/// A configuration for <see cref="Mixer"/>, in terms of input/output channels.
/// This can either be constructed explicitly (e.g. for an application which is aware
/// of the desired channels) or detected by the underlying API code.
/// </summary>
public sealed class MixerChannelConfiguration
{
    /// <summary>
    /// The input channels for the mixer.
    /// </summary>
    public IReadOnlyList<ChannelId> InputChannels { get; }

    /// <summary>
    /// The output channels for the mixer.
    /// </summary>
    public IReadOnlyList<ChannelId> OutputChannels { get; }

    /// <summary>
    /// The stereo pairings of input or output channels.
    /// </summary>
    public IReadOnlyList<StereoPair> StereoPairs { get; }

    /// <summary>
    /// The input channels, with stereo pairs represented as a single value.
    /// </summary>
    public IReadOnlyList<MonoOrStereoPairChannelId> PossiblyPairedInputs { get; }

    /// <summary>
    /// The output channels, with stereo pairs represented as a single value.
    /// </summary>
    public IReadOnlyList<MonoOrStereoPairChannelId> PossiblyPairedOutputs { get; }

    public MixerChannelConfiguration(
        IEnumerable<ChannelId> inputChannels,
        IEnumerable<ChannelId> outputChannels,
        IEnumerable<StereoPair> stereoPairs)
    {
        InputChannels = inputChannels.ToList().AsReadOnly();
        OutputChannels = outputChannels.ToList().AsReadOnly();
        StereoPairs = stereoPairs.ToList().AsReadOnly();

        var leftStereos = StereoPairs.ToDictionary(pair => pair.Left);
        var rightStereos = StereoPairs.Select(pair => pair.Right).ToHashSet();

        PossiblyPairedInputs = inputChannels.Select(GetPair)
            .OfType<MonoOrStereoPairChannelId>()
            .ToList()
            .AsReadOnly();
        PossiblyPairedOutputs = outputChannels.Select(GetPair)
            .OfType<MonoOrStereoPairChannelId>()
            .ToList()
            .AsReadOnly();

        MonoOrStereoPairChannelId? GetPair(ChannelId channelId) =>
            leftStereos.TryGetValue(channelId, out var pair) ? new MonoOrStereoPairChannelId(channelId, pair.Right, pair.Flags)
            : rightStereos.Contains(channelId) ? null
            : new MonoOrStereoPairChannelId(channelId, null, StereoFlags.None);
    }        
}
