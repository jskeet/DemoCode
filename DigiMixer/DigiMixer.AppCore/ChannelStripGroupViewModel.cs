using JonSkeet.CoreAppUtil;
using System.Collections.Immutable;

namespace DigiMixer.AppCore;

/// <summary>
/// Simple representation of a group of channels, each with an associated set of
/// faders. This is used to display group boxes, e.g. "Inputs" and "Outputs"
/// or just "Outputs" (but with faders per input).
/// </summary>
public class ChannelGroupViewModel<T> : ViewModelBase where T : IChannelViewModelBase
{
    public string Name { get; }
    public ImmutableArray<T> Channels { get; }

    private bool visible;
    public bool Visible
    {
        get => visible;
        set => SetProperty(ref visible, value);
    }

    internal ChannelGroupViewModel(string name, ImmutableArray<T> channels, bool initialVisibility)
    {
        Name = name;
        Channels = channels;
        Visible = initialVisibility;
    }
}
