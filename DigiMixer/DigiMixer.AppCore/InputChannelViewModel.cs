using DigiMixer.Core;
using NodaTime;
using System.Collections.Immutable;

namespace DigiMixer.AppCore;

public class InputChannelViewModel(ChannelMapping mapping, double? feedbackMutingThreshold, Duration? feedbackMutingDuration)
    : ChannelViewModelBase<InputChannel>(ChannelId.Input(mapping.Channel), mapping, feedbackMutingThreshold, feedbackMutingDuration)
{
    internal void SetFaders(ImmutableArray<OutputChannelViewModel> outputChannels)
    {
        Faders = [.. outputChannels.Select(output => new FaderViewModel(ChannelId, output.ChannelId, Id, output.Id, output.Appearance))];
    }

    protected override IEnumerable<InputChannel> GetChannels(Mixer mixer) => mixer.InputChannels;
}
