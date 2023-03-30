using DigiMixer.Core;
using System.ComponentModel;

namespace DigiMixer;

public abstract class ChannelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected PropertyChangedEventHandler? PropertyChangedHandler => PropertyChanged;

    public Mixer Mixer { get; }
    internal MonoOrStereoPairChannelId ChannelIdPair { get; }
    public ChannelId LeftOrMonoChannelId => ChannelIdPair.MonoOrLeftChannelId;
    public ChannelId? RightChannelId => ChannelIdPair.RightChannelId;
    public StereoFlags StereoFlags => ChannelIdPair.Flags;

    public bool IsStereo => RightChannelId is not null;

    private protected ChannelBase(Mixer mixer, MonoOrStereoPairChannelId channelIdPair)
    {
        Mixer = mixer;
        ChannelIdPair = channelIdPair;
        FallbackName = LeftOrMonoChannelId.ToString(RightChannelId);
    }

    /// <summary>
    /// The name to display if nothing else is known.
    /// </summary>
    public string FallbackName { get; }

    private MeterLevel meterLevel;
    public MeterLevel MeterLevel
    {
        get => meterLevel;
        internal set => this.SetProperty(PropertyChanged, ref meterLevel, value);
    }

    private MeterLevel stereoMeterLevel;
    public MeterLevel StereoMeterLevel
    {
        get => stereoMeterLevel;
        internal set => this.SetProperty(PropertyChanged, ref stereoMeterLevel, value);
    }

    private string? leftOrMonoName;
    internal string? LeftOrMonoName
    {
        get => leftOrMonoName;
        set
        {
            if (this.SetProperty(PropertyChanged, ref leftOrMonoName, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }

    private string? rightName;
    internal string? RightName
    {
        get => rightName;
        set
        {
            if (this.SetProperty(PropertyChanged, ref rightName, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }

    public string? Name
    {
        get
        {
            if (RightChannelId is null || (StereoFlags & StereoFlags.SplitNames) == 0)
            {
                return string.IsNullOrEmpty(LeftOrMonoName) ? null : LeftOrMonoName;
            }
            bool hasLeftName = !string.IsNullOrEmpty(LeftOrMonoName);
            bool hasRightName = !string.IsNullOrEmpty(RightName);
            return (hasLeftName, hasRightName) switch
            {
                (false, false) => null,
                (true, false) => LeftOrMonoName,
                (false, true) => RightName,
                (true, true) => $"{LeftOrMonoName} / {RightName}"
            };
        }
    }

    private bool muted;
    public bool Muted
    {
        get => muted;
        // We assume this will have the same value for both stereo channels even if it's
        // theoretically split.
        internal set => this.SetProperty(PropertyChanged, ref muted, value);
    }

    public void SetMuted(bool muted)
    {
        Mixer.SetMuted(LeftOrMonoChannelId, muted);
        if (ChannelIdPair.RightMuteId is ChannelId right)
        {
            Mixer.SetMuted(right, muted);
        }
    }
}
