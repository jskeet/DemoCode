using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DigiMixer;

/// <summary>
/// An input channel which receives information from an <see cref="IMixerApi"/>,
/// and can also transmit changes to it (e.g. for muting).
/// </summary>
public class InputChannel : ChannelBase, INotifyPropertyChanged
{
    public InputChannelId ChannelId { get; }
    public InputChannelId? StereoChannelId { get; }

    internal InputChannel(Mixer mixer, InputChannelId channelId, InputChannelId? stereoChannelId, IEnumerable<OutputChannelId> outputIds)
        : base(mixer, stereoChannelId.HasValue, channelId.ToString())
    {
        ChannelId = channelId;
        StereoChannelId = stereoChannelId;
        OutputMappings = outputIds.Select(oid => new InputOutputMapping(mixer, channelId, oid)).ToList().AsReadOnly();
    }

    /// <summary>
    /// The input/output mappings for this input channel, each of which has a separate fader level.
    /// </summary>
    public IReadOnlyList<InputOutputMapping> OutputMappings { get; }

    public override Task SetMuted(bool muted) => Mixer.Api.SetMuted(ChannelId, muted);
}
