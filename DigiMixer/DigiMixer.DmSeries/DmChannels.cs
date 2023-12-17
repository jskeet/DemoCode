using DigiMixer.Core;

namespace DigiMixer.DmSeries;

internal static class DmChannels
{
    internal const int MonoInputCount = 16;
    internal const int MonoOutputCount = 6;

    internal const byte StereoInLeftValue = 17;
    internal const byte StereoInRightValue = 18;

    internal static ChannelId Stereo1Left = ChannelId.Input(StereoInLeftValue);
    internal static ChannelId Stereo1Right = ChannelId.Input(StereoInRightValue);

    // TODO: Use the stereo inputs
    internal static IEnumerable<ChannelId> AllInputs => Enumerable.Range(1, MonoInputCount).Select(ChannelId.Input);
    internal static IEnumerable<ChannelId> AllOutputs => Enumerable.Range(1, MonoOutputCount).Select(ChannelId.Output).Append(ChannelId.MainOutputLeft).Append(ChannelId.MainOutputRight);
}
