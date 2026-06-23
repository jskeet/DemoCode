using DigiMixer.BehringerWing;
using DigiMixer.Core;
using DigiMixer.CqSeries;
using DigiMixer.DmSeries;
using DigiMixer.Mackie;
using DigiMixer.Osc;
using DigiMixer.QuSeries;
using DigiMixer.TfSeries;
using DigiMixer.UCNet;
using DigiMixer.UiHttp;
using JonSkeet.CoreAppUtil;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.ComponentModel;

namespace DigiMixer.AppCore;

#nullable disable warnings

/// <summary>
/// Configuration for the mixer.
/// </summary>
[Description("Top-level configuration for DigiMixer, which provides integration for audio mixers.")]
public class DigiMixerConfig
{
    /// <summary>
    /// Whether this is a fake mixer or not. This is populated on initialization.
    /// </summary>
    [JsonIgnore]
    public bool Fake { get; set; }

    /// <summary>
    /// Whether the mixer is enabled. This allows the fake to be set up even
    /// if the mixer is otherwise fully configured.
    /// </summary>
    [DefaultValue(true)]
    [Description("Whether or not the mixer is enabled, primarily to allow a fake to be used.")]
    public bool Enabled { get; set; } = true;

    [Description("IP address of the mixer.")]
    public string? Address { get; set; }
    [Description("TCP port of the mixer.")]
    public int? Port { get; set; }

    [Description("Type of audio mixer.")]
    public MixerHardwareType HardwareType { get; set; }

    /// <summary>
    /// The level (0-1) at which feedback is detected.
    /// Defaults to null, in which case feedback muting is disabled.
    /// </summary>
    [Description("When set, the level (between 0 and 1) at which feedback is detected. When not set, feedback muting is disabled.")]
    public double? FeedbackMutingThreshold { get; set; }

    /// <summary>
    /// The duration (in milliseconds) for which the input level must be at or above
    /// <see cref="FeedbackMutingThreshold"/> before the input channel is automatically muted.
    /// Defaults to 100 (1/10th of a second).
    /// </summary>
    [DefaultValue(100)]
    [Description("The number of milliseconds for which an input level must be at or above FeedbackMutingThreshold before the channel is automatically muted.")]
    public int FeedbackMutingMillis { get; set; } = 100;

    /// <summary>
    /// Name of the X-Touch Mini device as a MIDI port.
    /// </summary>
    [Description("The MIDI port name for the X-Touch Mini device to integrate with, if any")]
    public string? XTouchMiniDevice { get; set; }

    [DefaultValue(5)]
    [Description("The sensitivity of the X-Touch Mini rotary encoders for volume control.")]
    public int XTouchSensitivity { get; set; } = 5;

    /// <summary>
    /// Whether the main fader on the X-Touch Mini should be used for the main volume.
    /// </summary>
    [DefaultValue(true)]
    [Description("Whether the main fader (on the right) on the X-Touch Mini should be used for the main mixer volume.")]
    public bool XTouchMainVolumeEnabled { get; set; } = true;

    /// <summary>
    /// Name of the iCON Platform M+ device as a MIDI port.
    /// </summary>
    [JsonProperty("IconM+Device")]
    [Description("The MIDI port name for the iCON Platform M+ device to integrate with, if any")]
    public string? IconMPlusDevice { get; set; }

    /// <summary>
    /// Name of the iCON Platform X+ device as a MIDI port.
    /// </summary>
    [JsonProperty("IconX+Device")]
    [Description("The MIDI port name for the iCON Platform X+ device to integrate with, if any")]
    public string IconXPlusDevice { get; set; }

    [Description("Configuration for each input channel to be used. (Any irrelevant channels do not need to be configured.)")]
    public List<ChannelMapping> InputChannels { get; set; } = [];

    /// <summary>
    /// The configuration of mixer output channels. For a stereo output
    /// only the lower channel number is required. An output index
    /// of 0 means "main bus".
    /// </summary>
    [Description("Configuration for each output channel to be used. (Any irrelevant channels do not need to be configured.) The main output channel is expected to be the first entry in this list.")]
    public List<ChannelMapping> OutputChannels { get; set; } = [];

#nullable restore warnings

    public IMixerApi CreateMixerApi(ILogger logger, MixerApiOptions? options = null)
    {
        if (Fake || !Enabled)
        {
            return new FakeMixerApi(this);
        }
        return HardwareType switch
        {
            MixerHardwareType.Fake => new FakeMixerApi(this),
            MixerHardwareType.XAir => XAir.CreateMixerApi(logger, Address.OrThrow(), Port ?? 10024, options),
            MixerHardwareType.X32 => X32.CreateMixerApi(logger, Address.OrThrow(), Port ?? 10023, options),
            MixerHardwareType.SoundcraftUi => new UiHttpMixerApi(logger, Address.OrThrow(), Port ?? 80, options),
            MixerHardwareType.AllenHeathQu => QuMixer.CreateMixerApi(logger, Address.OrThrow(), Port ?? 51326, options),
            // We don't provide inbound port customization, but it would be easy to do if we ever needed to.
            MixerHardwareType.RcfM18 => Rcf.CreateMixerApi(logger, Address.OrThrow(), Port ?? 8000, options: options),
            MixerHardwareType.MackieDL => new MackieMixerApi(logger, Address.OrThrow(), Port ?? 50001, options),
            MixerHardwareType.StudioLive => StudioLive.CreateMixerApi(logger, Address.OrThrow(), Port ?? 53000, options),
            MixerHardwareType.AllenHeathCq => CqMixer.CreateMixerApi(logger, Address.OrThrow(), Port ?? 51326, options),
            MixerHardwareType.YamahaDm => DmMixer.CreateMixerApi(logger, Address.OrThrow(), Port ?? 50368, options),
            MixerHardwareType.YamahaTf => TfMixer.CreateMixerApi(logger, Address.OrThrow(), Port ?? 50368, options),
            MixerHardwareType.BehringerWing => WingMixer.CreateMixerApi(logger, Address.OrThrow(), Port ?? 2222, options),
            _ => throw new InvalidOperationException($"Unknown mixer type: {HardwareType}")
        };
    }

    [Description("The various types of audio mixer supported by DigiMixer.")]
    public enum MixerHardwareType
    {
        [Description("A fake mixer, used for testing when the real mixer isn't available.")]
        Fake,
        [Description("Behringer X-Air, e.g. XR-18")]
        XAir,
        [Description("Behringer X-Air, e.g. XR-18. (This is an alias for XAir.)")]
        BehringerXAir = XAir,
        // The M32C appears to be identical to the X32 in terms of OSC.
        [Description("Behringer X32 or M32.")]
        X32,
        [Description("Behringer X32 or M32. (This is an alias for X32.)")]
        M32 = X32,
        [Description("Midas M32. (This is an alias for X32.)")]
        MidasM32 = X32,
        [Description("Behringer X32. (This is an alias for X32.)")]
        BehringerX32 = X32,
        [Description("Soundcard Ui mixer, e.g. Ui24R")]
        SoundcraftUi,
        [Description("Allen and Heath Qu series, e.g. Qu-SB")]
        AllenHeathQu,
        [Description("RCF M 18")]
        RcfM18,
        [Description("Mackie DL series, e.g. DL32R")]
        MackieDL,
        [Description("PreSonus StudioLive series, e.g. 16R Series III")]
        StudioLive,
        [Description("Allen and Heath CQ series, e.g. CQ20B")]
        AllenHeathCq,
        [Description("Yamaha DM series, e.g. DM3")]
        YamahaDm,
        [Description("Yamaha TF series, e.g. TF-Rack")]
        YamahaTf,
        [Description("Behringer Wing")]
        BehringerWing
    }
}
