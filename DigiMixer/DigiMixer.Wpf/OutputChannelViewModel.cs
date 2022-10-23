using System.Windows.Media;

namespace DigiMixer.Wpf;

public class OutputChannelViewModel : ChannelViewModelBase<OutputChannel>
{
    public FaderViewModel Fader { get; }

    public OutputChannelViewModel(MonoOrStereoPairChannel<OutputChannel> pair) : base(pair, "id",  null)
    {
        Fader = new FaderViewModel(pair.MonoOrLeftChannel, Brushes.Transparent);
    }
}
