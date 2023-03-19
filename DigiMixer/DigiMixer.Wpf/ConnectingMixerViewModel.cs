using DigiMixer.Core;
using DigiMixer.Wpf.Utilities;

namespace DigiMixer.Wpf;

/// <summary>
/// ViewModel with the same properties as <see cref="MixerViewModel"/>, in order to allow
/// us to show a window while the *actual* mixer is being created.
/// </summary>
internal class ConnectingMixerViewModel : ViewModelBase
{
    public IReadOnlyList<InputChannelViewModel> InputChannels { get; } = new List<InputChannelViewModel>();
    public IReadOnlyList<OutputChannelViewModel> OutputChannels { get; } = new List<OutputChannelViewModel>();

    public MixerInfo MixerInfo { get; } = new MixerInfo("(Connecting...)", "(Connecting...)", "(Connecting...)");

    public string ConnectionStatus => "(Connecting...)";
}
