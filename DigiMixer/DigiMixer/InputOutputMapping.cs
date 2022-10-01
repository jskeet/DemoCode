using DigiMixer;
using System.ComponentModel;

namespace DigiMixer;

/// <summary>
/// A mapping for an input to an output, with a fader level.
/// </summary>
public class InputOutputMapping : INotifyPropertyChanged
{
    private readonly Mixer mixer;

    public InputChannelId InputChannelId { get; }
    public OutputChannelId OutputChannelId { get; }

    internal InputOutputMapping(Mixer mixer, InputChannelId inputId, OutputChannelId outputId)
    {
        this.mixer = mixer;
        InputChannelId = inputId;
        OutputChannelId = outputId;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private FaderLevel faderLevel;
    public FaderLevel FaderLevel
    {
        get => faderLevel;
        set => this.SetProperty(PropertyChanged, ref faderLevel, value);
    }

    public Task SetFaderLevel(FaderLevel faderLevel) =>
        mixer.Api.SetFaderLevel(InputChannelId, OutputChannelId, faderLevel);
}
