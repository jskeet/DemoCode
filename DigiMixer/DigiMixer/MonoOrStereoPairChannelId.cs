using DigiMixer.Core;

namespace DigiMixer;

// TODO: Find a better name than this!
// TODO: How do we validate the inputs? Possible limitation of records.
// TODO: Expose this as ChannelBase rather than ChannelId? Would be more useful. (Possibly a separate type?)
public record MonoOrStereoPairChannelId(
    ChannelId MonoOrLeftChannelId,
    ChannelId? RightChannelId,
    StereoFlags Flags)
{
    /// <summary>
    /// The channel ID for the right fader, or null if
    /// either <see cref="Flags"/> doesn't include <see cref="StereoFlags.SplitFaders"/>,
    /// or <see cref="RightChannelId"/> is null.
    /// </summary>
    public ChannelId? RightFaderId =>
        (Flags & StereoFlags.SplitFaders) != 0 ? RightChannelId : null;

    /// <summary>
    /// The channel ID for the right fader, or null if
    /// either <see cref="Flags"/> doesn't include <see cref="StereoFlags.SplitMutes"/>,
    /// or <see cref="RightMuteId"/> is null.
    /// </summary>
    public ChannelId? RightMuteId =>
        (Flags & StereoFlags.SplitMutes) != 0 ? RightChannelId : null;
}
