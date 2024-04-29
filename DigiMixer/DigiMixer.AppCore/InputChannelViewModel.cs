using DigiMixer.Core;
using JonSkeet.CoreAppUtil;
using NodaTime;

namespace DigiMixer.Controls;

public class InputChannelViewModel : ChannelViewModelBase<InputChannel>
{
    public InputChannelViewModel(ChannelMapping mapping, double? feedbackMutingThreshold, Duration? feedbackMutingDuration)
        : base(ChannelId.Input(mapping.Channel), mapping, feedbackMutingThreshold, feedbackMutingDuration)
    {
    }

    internal void SetFaders(IReadOnlyList<OutputChannelViewModel> outputChannels)
    {
        Faders = outputChannels
            .ToReadOnlyList(output => new FaderViewModel(ChannelId, output.ChannelId, Id, output.Id, output.Appearance));
    }

    protected override IEnumerable<InputChannel> GetChannels(Mixer mixer) => mixer.InputChannels;
}
