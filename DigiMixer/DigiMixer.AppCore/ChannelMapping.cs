using Newtonsoft.Json;

namespace DigiMixer.AppCore;

/// <summary>
/// A mapping between an configuration channel ID (text-based, user-configurable)
/// and a DigiMixer channel ID (largely opaque integer), with an optional display name.
/// </summary>
public class ChannelMapping
{
    /// <summary>
    /// The mixer channel ID (for the relevant input or output).
    /// The precise numbers available will depend on hardware.
    /// </summary>
    public int Channel { get; set; }

    /// <summary>
    /// The ID used within the configuration file.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// An optional display name.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// The background colour to use (or leave empty to use the default).
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Whether the channel should be shown by default to start with.
    /// </summary>
    public bool InitiallyVisible { get; set; }

    /// <summary>
    /// Optional short name to use for constrained UIs (e.g. digital scribble strips).
    /// </summary>
    public string ShortName { get; set; }

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
    public bool Foldback { get; set; }

    public override string ToString() => DisplayName is null ? Id : $"{DisplayName} ({Id})";
}
