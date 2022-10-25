using System.ComponentModel;

namespace DigiMixer;

/// <summary>
/// A mapping for an input to an output, with a fader level.
/// </summary>
public class InputOutputMapping : IFader, INotifyPropertyChanged
{
    private readonly Mixer mixer;

    private MonoOrStereoPairChannelId inputIdPair;
    private MonoOrStereoPairChannelId outputIdPair;

    internal ChannelId PrimaryInputChannelId => inputIdPair.MonoOrLeftChannelId;
    internal ChannelId PrimaryOutputChannelId => outputIdPair.MonoOrLeftChannelId;

    internal InputOutputMapping(Mixer mixer, MonoOrStereoPairChannelId inputIdPair, MonoOrStereoPairChannelId outputIdPair)
    {
        this.mixer = mixer;
        this.inputIdPair = inputIdPair;
        this.outputIdPair = outputIdPair;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private FaderLevel faderLevel;
    public FaderLevel FaderLevel
    {
        get => faderLevel;
        internal set => this.SetProperty(PropertyChanged, ref faderLevel, value);
    }

    public async Task SetFaderLevel(FaderLevel level)
    {
        await mixer.Api.SetFaderLevel(PrimaryInputChannelId, PrimaryOutputChannelId, level);
        var rightInputId = inputIdPair.RightFaderId;
        if (rightInputId is not null)
        {
            await mixer.Api.SetFaderLevel(rightInputId.Value, PrimaryOutputChannelId, level);
        }

        if (outputIdPair.RightFaderId is ChannelId rightOutputId)
        {
            await mixer.Api.SetFaderLevel(PrimaryInputChannelId, rightOutputId, level);
            if (rightInputId is not null)
            {
                await mixer.Api.SetFaderLevel(rightInputId.Value, rightOutputId, level);
            }
        }
    }

    public override string ToString() => $"{PrimaryInputChannelId} => {PrimaryOutputChannelId}";
}
