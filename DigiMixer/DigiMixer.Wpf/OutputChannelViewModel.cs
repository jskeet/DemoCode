using System.Windows.Media;

namespace DigiMixer.Wpf;

public class OutputChannelViewModel : ChannelViewModelBase<OutputChannel>
{
    public FaderViewModel Fader { get; }

    public OutputChannelViewModel(OutputChannel model) : base(model, "id", null)
    {
        Fader = new FaderViewModel(model, model, Brushes.Transparent);
    }
}
