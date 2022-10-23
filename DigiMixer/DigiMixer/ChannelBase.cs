using System.ComponentModel;

namespace DigiMixer;

public abstract class ChannelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected PropertyChangedEventHandler? PropertyChangedHandler => PropertyChanged;

    public Mixer Mixer { get; }
    public ChannelId ChannelId { get; }

    protected ChannelBase(Mixer mixer, ChannelId channelId)
    {
        Mixer = mixer;
        ChannelId = channelId;
        FallbackName = channelId.ToString();
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
