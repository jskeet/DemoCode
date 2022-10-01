using System.ComponentModel;

namespace DigiMixer;

public abstract class ChannelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected PropertyChangedEventHandler? PropertyChangedHandler => PropertyChanged;

    protected Mixer Mixer { get; }

    protected ChannelBase(Mixer mixer)
    {
        Mixer = mixer;
        meterLevel = MeterLevel.MinValue;
        stereoMeterLevel = MeterLevel.MinValue;
    }

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

    private string? name;
    public string? Name
    {
        get => name;
        set => this.SetProperty(PropertyChanged, ref name, value);
    }

    private bool muted;
    public bool Muted
    {
        get => muted;
        set => this.SetProperty(PropertyChanged, ref muted, value);
    }
}
