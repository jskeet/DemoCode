using DigiMixer.Core;
using DigiMixer.CqSeries;
using DigiMixer.DmSeries;
using DigiMixer.Mackie;
using DigiMixer.Osc;
using DigiMixer.QuSeries;
using DigiMixer.UCNet;
using DigiMixer.UiHttp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.ComponentModel;

namespace DigiMixer.Controls;

/// <summary>
/// Configuration for the mixer.
/// </summary>
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
    public bool Enabled { get; set; } = true;

    public string Address { get; set; }
    public int? Port { get; set; }

    public MixerHardwareType HardwareType { get; set; }

    /// <summary>
    /// The level (0-1) at which feedback is detected.
    /// Defaults to null, in which case feedback muting is disabled.
    /// </summary>
    public double? FeedbackMutingThreshold { get; set; }

    /// <summary>
    /// The duration (in milliseconds) for which the input level must be at or above
    /// <see cref="FeedbackMutingThreshold"/> before the input channel is automatically muted.
    /// Defaults to 100 (1/10th of a second).
    /// </summary>
    [DefaultValue(100)]
    public int FeedbackMutingMillis { get; set; } = 100;

    /// <summary>
    /// Name of the X-Touch Mini device as a MIDI port.
    /// </summary>
    public string XTouchMiniDevice { get; set; }

    [DefaultValue(5)]
    public int XTouchSensitivity { get; set; } = 5;

    /// <summary>
    /// Whether the main fader on the X-Touch Mini should be used for the main volume.
    /// </summary>
    [DefaultValue(true)]
    public bool XTouchMainVolumeEnabled { get; set; } = true;

    /// <summary>
    /// Name of the iCON Platform M+ device as a MIDI port.
    /// </summary>
    [JsonProperty("IconM+Device")]
    public string IconMPlusDevice { get; set; }

    /// <summary>
    /// Name of the iCON Platform X+ device as a MIDI port.
    /// </summary>
    [JsonProperty("IconX+Device")]
    public string IconXPlusDevice { get; set; }

    public List<ChannelMapping> InputChannels { get; set; } = new List<ChannelMapping>();

    /// <summary>
    /// The display names of mixer output channels. For a stereo output
    /// only the lower channel number is required. An output index
    /// of 0 means "main bus".
    /// </summary>
    public List<ChannelMapping> OutputChannels { get; set; } = new List<ChannelMapping>();

    public IMixerApi CreateMixerApi(ILogger logger)
    {
        if (Fake || !Enabled)
        {
            return new FakeMixerApi(this);
        }
        return HardwareType switch
        {
            MixerHardwareType.Fake => new FakeMixerApi(this),
            MixerHardwareType.XAir => XAir.CreateMixerApi(logger, Address, Port ?? 10024),
            MixerHardwareType.X32 => X32.CreateMixerApi(logger, Address, Port ?? 10023),
            MixerHardwareType.SoundcraftUi => new UiHttpMixerApi(logger, Address, Port ?? 80),
            MixerHardwareType.AllenHeathQu => QuMixer.CreateMixerApi(logger, Address, Port ?? 51326),
            // We don't provide inbound port customization, but it would be easy to do if we ever needed to.
            MixerHardwareType.RcfM18 => Rcf.CreateMixerApi(logger, Address, Port ?? 8000),
            MixerHardwareType.MackieDL => new MackieMixerApi(logger, Address, Port ?? 50001),
            MixerHardwareType.StudioLive => StudioLive.CreateMixerApi(logger, Address, Port ?? 53000),
            MixerHardwareType.AllenHeathCq => CqMixer.CreateMixerApi(logger, Address, Port ?? 51326),
            MixerHardwareType.YamahaDm => DmMixer.CreateMixerApi(logger, Address, Port ?? 50368),
            _ => throw new InvalidOperationException($"Unknown mixer type: {HardwareType}")
        };
    }

    public enum MixerHardwareType
    {
        Fake,
        XAir,
        BehringerXAir = XAir,
        // The M32C appears to be identical to the X32 in terms of OSC.
        X32,
        M32 = X32,
        MidasM32 = X32,
        BehringerX32 = X32,
        SoundcraftUi,
        AllenHeathQu,
        RcfM18,
        MackieDL,
        StudioLive,
        AllenHeathCq,
        YamahaDm
    }
}
