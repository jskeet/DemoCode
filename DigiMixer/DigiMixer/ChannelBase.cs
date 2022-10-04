using System.ComponentModel;

namespace DigiMixer;

public abstract class ChannelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected PropertyChangedEventHandler? PropertyChangedHandler => PropertyChanged;

    public Mixer Mixer { get; }

    protected ChannelBase(Mixer mixer, bool hasStereoMeterLevel, string fallbackName)
    {
        Mixer = mixer;
        meterLevel = MeterLevel.MinValue;
        stereoMeterLevel = MeterLevel.MinValue;
        HasStereoMeterLevel = hasStereoMeterLevel;
        FallbackName = fallbackName;
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

    public bool HasStereoMeterLevel { get; }

    private MeterLevel stereoMeterLevel;
    public MeterLevel StereoMeterLevel
    {
        get => stereoMeterLevel;
        internal set => this.SetProperty(PropertyChanged, ref stereoMeterLevel, value);
    }

    private string? name;
    public string? Name
    {
        get => name;
        internal set => this.SetProperty(PropertyChanged, ref name, value);
    }

    private bool muted;
    public bool Muted
    {
        get => muted;
        internal set => this.SetProperty(PropertyChanged, ref muted, value);
    }

    public abstract Task SetMuted(bool muted);
}
