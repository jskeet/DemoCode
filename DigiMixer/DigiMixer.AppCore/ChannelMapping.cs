using Newtonsoft.Json;
using System.ComponentModel;

namespace DigiMixer.AppCore;

#nullable disable warnings

/// <summary>
/// A mapping between an configuration channel ID (text-based, user-configurable)
/// and a DigiMixer channel ID (largely opaque integer), with an optional display name.
/// </summary>
[Description("Configuration for a DigiMixer channel, including a mapping from a user-facing ID (e.g. 'Lectern') to a DigiMixer numeric channel")]
public class ChannelMapping
{
    /// <summary>
    /// The mixer channel ID (for the relevant input or output).
    /// The precise numbers available will depend on hardware.
    /// </summary>
    [Description("The DigiMixer numeric channel ID. Precise values will depend on the hardware; contact skeet@pobox.com for more details.")]
    public int Channel { get; set; }

    /// <summary>
    /// The ID used within the configuration file.
    /// </summary>
    [Description("The identifier used to refer to this channel within other configuration, e.g. in At Your Service. Separating this logical ID from the hardware-based ID allows channels to be remapped without changing as much configuration.")]
    public string Id { get; set; }

    /// <summary>
    /// An optional display name.
    /// </summary>
    [Description("Optional display name for the channel. When this isn't specified, the ID is displayed.")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// The background colour to use (or leave empty to use the default).
    /// </summary>
    [Description("The background colour to use for the channel. Leave this empty to use a default colour.")]
    public string? Color { get; set; }

    /// <summary>
    /// Whether the channel should be shown by default to start with.
    /// </summary>
    [Description("Whether or not this channel should be shown in the user interface by default.")]
    public bool InitiallyVisible { get; set; }

    /// <summary>
    /// Optional short name to use for constrained UIs (e.g. digital scribble strips).
    /// </summary>
    [Description("Optional short name to use for constrained UIs(e.g.digital scribble strips).")]
    public string? ShortName { get; set; }

    /// <summary>
    /// The effective display name, which is <see cref="DisplayName"/> with a fallback to <see cref="Id"/>.
    /// </summary>
    [JsonIgnore]
    public string EffectiveDisplayName => DisplayName ?? Id;

    /// <summary>
    /// The effective short name, which is <see cref="ShortName"/> with a fallback to <see cref="EffectiveDisplayName"/>.
    /// </summary>
    [JsonIgnore]
    public string EffectiveShortName => ShortName ?? EffectiveDisplayName;

    /// <summary>
    /// For output channels, set to true if this is a foldback channel.
    /// Foldback channels can be muted/unmuted on a per-scene basis.
    /// </summary>
    [Description("Whether this is a foldback channel (e.g. a musician's monitor).")]
    public bool Foldback { get; set; }

    public override string ToString() => DisplayName is null ? Id : $"{DisplayName} ({Id})";
}
