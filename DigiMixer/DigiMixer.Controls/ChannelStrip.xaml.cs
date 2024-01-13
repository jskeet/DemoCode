using System.Windows.Controls;

namespace DigiMixer.Controls;

/// <summary>
/// A control representing a "channel strip": a sequence of faders corresponding
/// to a single channel. This could be an output channel with corresponding input
/// faders and an "overall" fader, or an input channel with corresponding faders
/// for that input against each output.
/// </summary>
public partial class ChannelStrip : UserControl
{
    public ChannelStrip()
    {
        InitializeComponent();
    }
}
