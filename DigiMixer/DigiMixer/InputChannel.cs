using System.ComponentModel;
using System.Linq;

namespace DigiMixer;

/// <summary>
/// An input channel which receives information from an <see cref="IMixerApi"/>,
/// and can also transmit changes to it (e.g. for muting).
/// </summary>
public class InputChannel : ChannelBase, INotifyPropertyChanged
{
    internal InputChannel(Mixer mixer, MonoOrStereoPairChannelId channelIdPair, IEnumerable<OutputChannel> outputChannels) : base(mixer, channelIdPair)
    {
        OutputMappings = outputChannels.Select(output => new InputOutputMapping(mixer, this, output)).ToList().AsReadOnly();
    }

    /// <summary>
    /// The input/output mappings for this input channel, each of which has a separate fader level.
    /// </summary>
    public IReadOnlyList<InputOutputMapping> OutputMappings { get; }
}
