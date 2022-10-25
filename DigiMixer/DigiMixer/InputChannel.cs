using System.ComponentModel;
using System.Linq;

namespace DigiMixer;

/// <summary>
/// An input channel which receives information from an <see cref="IMixerApi"/>,
/// and can also transmit changes to it (e.g. for muting).
/// </summary>
public class InputChannel : ChannelBase, INotifyPropertyChanged
{
    internal InputChannel(Mixer mixer, MonoOrStereoPairChannelId channelIdPair, IEnumerable<MonoOrStereoPairChannelId> outputIdPairs) : base(mixer, channelIdPair)
    {
        OutputMappings = outputIdPairs.Select(oidPair => new InputOutputMapping(mixer, channelIdPair, oidPair)).ToList().AsReadOnly();
    }

    /// <summary>
    /// The input/output mappings for this input channel, each of which has a separate fader level.
    /// </summary>
    public IReadOnlyList<InputOutputMapping> OutputMappings { get; }
}
