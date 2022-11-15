using DigiMixer.Core;

namespace DigiMixer.Mackie;

internal static class MackieAddresses
{
    /// <summary>
    /// The output channel ID for the left side of the main output.
    /// </summary>
    internal static ChannelId MainOutputLeft { get; } = ChannelId.Output(100);

    /// <summary>
    /// The output channel ID for the right side of the main output.
    /// </summary>
    internal static ChannelId MainOutputRight { get; } = ChannelId.Output(101);

    /// <summary>
    /// The input channel ID for the "return 1" input.
    /// </summary>
    internal static ChannelId Return1 { get; } = ChannelId.Input(17);

    /// <summary>
    /// The input channel ID for the "return 2" input.
    /// </summary>
    internal static ChannelId Return2 { get; } = ChannelId.Input(18);

    internal static int GetFaderAddress(ChannelId inputId, ChannelId outputId)
    {
        int inputBase = GetInputOrigin(inputId);
        int offset = outputId == MainOutputLeft ? 8
            : outputId.Value * 3 + 0x2f;
        return inputBase + offset;
    }

    internal static int GetFaderAddress(ChannelId outputId) =>
        outputId == MainOutputLeft ? 0x09d7 : (outputId.Value - 1) * 90 + 0x0a32;

    internal static int GetMuteAddress(ChannelId channelId) =>
        channelId.IsInput ? GetInputOrigin(channelId) + 7
        : channelId == MainOutputLeft ? 0x09d8
        : (channelId.Value - 1) * 90 + 0x0a31;

    internal static int GetNameAddress(ChannelId channelId) =>
        channelId.IsInput ? channelId.Value
        : channelId == MainOutputLeft ? 0x21
        : channelId.Value + 0x21;

    private static int GetInputOrigin(ChannelId inputId) => (inputId.Value - 1) * 100 + 1;
}
