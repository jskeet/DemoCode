using DigiMixer.Core;

namespace DigiMixer;

// TODO: Potentially get rid of this in the future. It's not too bad now it's internal, at least.
internal record MonoOrStereoPairChannelId(
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
