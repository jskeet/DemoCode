using JonSkeet.CoreAppUtil;
using DigiMixer;
using DigiMixer.AppCore;
using DigiMixer.Core;
using System.ComponentModel;

namespace DigiMixer.AppCore;

// Note on fader appearances:
// - Input-specific faders are always paired with a specific output channel (which may be "main")
// - An output fader may be the "overall" fader for the output, without a specific input.
// Faders are shown in three groups:
// - Grouped by input, where only selected outputs are included. These faders always use the colour
//   of the output.
// - Grouped by output, where only selected inputs are included, as well as the overall output.
//   These faders use the colour of the input, or white for the overall output, which is always shown.
// - Just the overall outputs, where all output channels are shown, all with a white background.
//
// Corollary: the appearance of an "overall" fader is always white, and always shown.

/// <summary>
/// View model for a fader; it's always associated with an output channel,
/// but may also be specific to an input channel.
/// </summary>
public class FaderViewModel : ViewModelBase
{
    private IFader fader;
    [RelatedProperties(nameof(FaderLevel), nameof(MaxFaderLevel))]
    private IFader Fader
    {
        get => fader;
        set
        {
            var oldFader = fader;
            if (SetProperty(ref fader, value))
            {
                Notifications.MaybeUnsubscribe(oldFader, HandleFaderPropertyChanged);
                Notifications.MaybeSubscribe(fader, HandleFaderPropertyChanged);
            }
        }
    }

    public ChannelAppearance Appearance { get; }

    [RelatedProperties(nameof(FaderLevelDb), nameof(FaderLevelPercentage))]
    public int FaderLevel
    {
        get => Fader?.FaderLevel.Value ?? 0;
        set => Fader?.SetFaderLevel(new(value));
    }

    public double FaderLevelDb => NormalizeSmallNegativeToZero(fader?.Scale.ConvertToDb(FaderLevel) ?? 0);
    public int MaxFaderLevel => fader?.Scale.MaxValue ?? 1_000_000;

    // The config-based IDs.
    public string InputId { get; }
    public string OutputId { get; }

    // The DigiMixer-based IDs.
    public ChannelId? InputChannelId { get; }
    public ChannelId OutputChannelId { get; }

    /// <summary>
    /// The level of the fader, as a percentage.
    /// </summary>
    public string FaderLevelPercentage => $"{FaderLevel * 100 / MaxFaderLevel}%";

    internal FaderViewModel(ChannelId? inputChannelId, ChannelId outputChannelId, string inputId, string outputId, ChannelAppearance channelAppearance)
    {
        InputChannelId = inputChannelId;
        OutputChannelId = outputChannelId;
        InputId = inputId;
        OutputId = outputId;
        Appearance = channelAppearance;
    }

    internal void PopulateFromMixer(Mixer mixer)
    {
        if (InputChannelId is ChannelId inputId)
        {
            var input = mixer.InputChannels.FirstOrDefault(ch => ch.LeftOrMonoChannelId == inputId);
            Fader = input?.OutputMappings.FirstOrDefault(mapping => mapping.OutputChannel.LeftOrMonoChannelId == OutputChannelId);
        }
        else
        {
            Fader = mixer.OutputChannels.FirstOrDefault(ch => ch.LeftOrMonoChannelId == OutputChannelId);
        }
    }

    private void HandleFaderPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IFader.FaderLevel))
        {
            RaisePropertyChanged(nameof(FaderLevel));
            RaisePropertyChanged(nameof(FaderLevelPercentage));
            RaisePropertyChanged(nameof(FaderLevelDb));
        }
    }

    /// <summary>
    /// If we just format the raw "value" we see -0.0 quite a lot (e.g. for -0.05).
    /// With this method, if the 1dp value is going to be rounded to 0.0 anyway, we omit the sign.
    /// </summary>
    private static double NormalizeSmallNegativeToZero(double value) =>
        -0.1 < value && value < 0 ? 0.0 : value;
}
