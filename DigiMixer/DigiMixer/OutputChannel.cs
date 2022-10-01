using System.ComponentModel;

namespace DigiMixer;

/// <summary>
/// An output channel which receives information from an <see cref="IMixerApi"/>,
/// and can also transmit changes to it (e.g. for muting).
/// </summary>
public class OutputChannel : ChannelBase, INotifyPropertyChanged
{
    public OutputChannelId ChannelId { get; }
    public OutputChannelId? StereoChannelId { get; }

    public OutputChannel(Mixer mixer, OutputChannelId channelId, OutputChannelId? stereoChannelId) : base(mixer)
    {
        ChannelId = channelId;
        StereoChannelId = stereoChannelId;
    }

    private FaderLevel faderLevel;
    public FaderLevel FaderLevel
    {
        get => faderLevel;
        set => this.SetProperty(PropertyChangedHandler, ref faderLevel, value);
    }

    public Task SetFaderLevel(FaderLevel faderLevel) => Mixer.Api.SetFaderLevel(ChannelId, faderLevel);
}
