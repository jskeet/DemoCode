namespace DigiMixer;

/// <summary>
/// A configuration for <see cref="Mixer"/>, in terms of input/output channels.
/// This can either be constructed explicitly (e.g. for an application which is aware
/// of the desired channels) or detected by the underlying API code.
/// </summary>
public sealed class MixerChannelConfiguration
{
    /// <summary>
    /// The input channels for the mixer. For mono inputs, <code>left</code> is non-null and
    /// <code>right</code> is null; for stereo inputs both values are non-null.
    /// </summary>
    public IReadOnlyList<(InputChannelId left, InputChannelId? right)> InputChannels { get; }

    /// <summary>
    /// The output channels for the mixer. For mono inputs, <code>left</code> is non-null and
    /// <code>right</code> is null; for stereo inputs both values are non-null.
    /// </summary>
    public IReadOnlyList<(OutputChannelId left, OutputChannelId? right)> OutputChannels { get; }

    public MixerChannelConfiguration(
        IEnumerable<(InputChannelId left, InputChannelId? right)> inputChannels,
        IEnumerable<(OutputChannelId left, OutputChannelId? right)> outputChannels)
    {
        InputChannels = inputChannels.ToList().AsReadOnly();
        OutputChannels = outputChannels.ToList().AsReadOnly();
    }
}
