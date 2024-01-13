using JonSkeet.WpfUtil;

namespace DigiMixer.Controls;

/// <summary>
/// Simple representation of a group of channels, each with an associated set of
/// faders. This is used to display group boxes, e.g. "Inputs" and "Outputs"
/// or just "Outputs" (but with faders per input).
/// </summary>
public class ChannelGroupViewModel : ViewModelBase
{
    public string Name { get; }
    public IReadOnlyList<IChannelViewModelBase> Channels { get; }

    private bool visible;
    public bool Visible
    {
        get => visible;
        set => SetProperty(ref visible, value);
    }

    internal ChannelGroupViewModel(string name, IReadOnlyList<IChannelViewModelBase> channels, bool initialVisibility)
    {
        Name = name;
        Channels = channels;
        Visible = initialVisibility;
    }
}
