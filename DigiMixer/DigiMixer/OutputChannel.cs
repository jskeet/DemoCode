using System.ComponentModel;

namespace DigiMixer;

/// <summary>
/// An output channel which receives information from an <see cref="IMixerApi"/>,
/// and can also transmit changes to it (e.g. for muting).
/// </summary>
public class OutputChannel : ChannelBase, IFader, INotifyPropertyChanged
{
    public OutputChannel(Mixer mixer, ChannelId channelId) : base(mixer, channelId)
    {
    }

    private FaderLevel faderLevel;
    public FaderLevel FaderLevel
    {
        get => faderLevel;
        internal set => this.SetProperty(PropertyChangedHandler, ref faderLevel, value);
    }

    public Task SetFaderLevel(FaderLevel level) => Mixer.Api.SetFaderLevel(ChannelId, level);

    public override Task SetMuted(bool muted) => Mixer.Api.SetMuted(ChannelId, muted);
}
