using System.ComponentModel;

namespace DigiMixer;

/// <summary>
/// An input channel which receives information from an <see cref="IMixerApi"/>,
/// and can also transmit changes to it (e.g. for muting).
/// </summary>
public class InputChannel : ChannelBase, INotifyPropertyChanged
{
    internal InputChannel(Mixer mixer, ChannelId channelId, IEnumerable<ChannelId> outputIds) : base(mixer, channelId)
    {
        OutputMappings = outputIds.Select(oid => new InputOutputMapping(mixer, channelId, oid)).ToList().AsReadOnly();
    }

    /// <summary>
    /// The input/output mappings for this input channel, each of which has a separate fader level.
    /// </summary>
    public IReadOnlyList<InputOutputMapping> OutputMappings { get; }

    public override Task SetMuted(bool muted) => Mixer.Api.SetMuted(ChannelId, muted);
}
