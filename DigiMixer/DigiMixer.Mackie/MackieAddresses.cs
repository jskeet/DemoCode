using DigiMixer.Core;

namespace DigiMixer.Mackie;

internal static class MackieAddresses
{
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
        int offset = outputId.IsMainOutput ? 8 : outputId.Value * 3 + 0x2f;
        return inputBase + offset;
    }

    internal static int GetFaderAddress(ChannelId outputId) =>
        outputId.IsMainOutput ? 0x09d7 : GetOutputOrigin(outputId) + 1;

    internal static int GetMuteAddress(ChannelId channelId) =>
        channelId.IsInput ? GetInputOrigin(channelId) + 0x07
        : channelId.IsMainOutput ? 0x09d8
        : GetOutputOrigin(channelId) + 0;

    internal static int GetNameAddress(ChannelId channelId) =>
        channelId.IsInput ? channelId.Value
        : channelId.IsMainOutput ? 0x21
        : channelId.Value + 0x21;

    internal static int GetStereoLinkAddress(ChannelId channelId) =>
        channelId.IsInput
        ? GetInputOrigin(channelId) + 0x0b
        : GetOutputOrigin(channelId) + 0x04;

    private static int GetInputOrigin(ChannelId inputId) => (inputId.Value - 1) * 100 + 1;
    private static int GetOutputOrigin(ChannelId outputId) => (outputId.Value - 1) * 90 + 0x0a31;
}
