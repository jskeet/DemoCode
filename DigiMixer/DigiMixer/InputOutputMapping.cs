using DigiMixer.Core;
using System.ComponentModel;

namespace DigiMixer;

/// <summary>
/// A mapping for an input to an output, with a fader level.
/// </summary>
public class InputOutputMapping : IFader, INotifyPropertyChanged
{
    private readonly Mixer mixer;

    public InputChannel InputChannel { get; }
    public OutputChannel OutputChannel { get; }

    internal InputOutputMapping(Mixer mixer, InputChannel inputChannel, OutputChannel outputChannel)
    {
        this.mixer = mixer;
        InputChannel = inputChannel;
        OutputChannel = outputChannel;
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
        await mixer.Api.SetFaderLevel(InputChannel.LeftOrMonoChannelId, OutputChannel.LeftOrMonoChannelId, level);
        var rightInputId = InputChannel.ChannelIdPair.RightFaderId;
        if (rightInputId is not null)
        {
            await mixer.Api.SetFaderLevel(rightInputId.Value, OutputChannel.LeftOrMonoChannelId, level);
        }

        if (OutputChannel.ChannelIdPair.RightFaderId is ChannelId rightOutputId)
        {
            await mixer.Api.SetFaderLevel(InputChannel.LeftOrMonoChannelId, rightOutputId, level);
            if (rightInputId is not null)
            {
                await mixer.Api.SetFaderLevel(rightInputId.Value, rightOutputId, level);
            }
        }
    }

    public override string ToString() => $"{InputChannel.Name ?? InputChannel.FallbackName} => {OutputChannel.Name ?? OutputChannel.FallbackName}";
}
