using DigiMixer.Core;
using System.ComponentModel;

namespace DigiMixer;

/// <summary>
/// An output channel which receives information from an <see cref="IMixerApi"/>,
/// and can also transmit changes to it (e.g. for muting).
/// </summary>
public class OutputChannel : ChannelBase, IFader, INotifyPropertyChanged
{
    public OutputChannel(Mixer mixer, MonoOrStereoPairChannelId channelIdPair) : base(mixer, channelIdPair)
    {
    }

    private FaderLevel faderLevel;
    public FaderLevel FaderLevel
    {
        get => faderLevel;
        internal set => this.SetProperty(PropertyChangedHandler, ref faderLevel, value);
    }

    public void SetFaderLevel(FaderLevel level)
    {
        Mixer.ApiWrapper.SetFaderLevel(LeftOrMonoChannelId, level);
        if ((StereoFlags & StereoFlags.SplitMutes) != 0 && RightChannelId is ChannelId right)
        {
            Mixer.ApiWrapper.SetFaderLevel(right, level);
        }
    }
}
