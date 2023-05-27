using DigiMixer.Core;
using DigiMixer.Mackie.Core;
using System.Text;

namespace DigiMixer.Mackie;

internal class DL16SProfile : IMixerProfile
{
    internal static DL16SProfile Instance { get; } = new DL16SProfile();

    private DL16SProfile()
    {
    }

    public int InputChannelCount => 18;
    public int AuxChannelCount => 6;
    public byte ModelNameInfoRequest => 0x12;

    public string GetModelName(MackiePacket modelInfo)
    {
        var data = modelInfo.Body.InSequentialOrder().Data;
        // The use of 16 here is somewhat arbitrary... there's lots we don't understand in this packet.
        return Encoding.UTF8.GetString(data.Slice(8, 16)).TrimEnd('\0');
    }

    public int GetFaderAddress(ChannelId inputId, ChannelId outputId)
    {
        int inputBase = GetInputOrigin(inputId);
        int offset = outputId.IsMainOutput ? 8 : outputId.Value * 3 + 0x2f;
        return inputBase + offset;
    }

    public int GetFaderAddress(ChannelId outputId) =>
        outputId.IsMainOutput ? 0x09d7 : GetOutputOrigin(outputId) + 1;

    public int GetMuteAddress(ChannelId channelId) =>
        channelId.IsInput ? GetInputOrigin(channelId) + 0x07
        : channelId.IsMainOutput ? 0x09d8
        : GetOutputOrigin(channelId) + 0;

    public int GetNameAddress(ChannelId channelId) =>
        channelId.IsInput ? channelId.Value
        : channelId.IsMainOutput ? 0x21
        : channelId.Value + 0x21;

    public int GetStereoLinkAddress(ChannelId channelId) =>
        channelId.IsInput
        ? GetInputOrigin(channelId) + 0x0b
        : GetOutputOrigin(channelId) + 0x04;

    private static int GetInputOrigin(ChannelId inputId) => (inputId.Value - 1) * 100 + 1;
    private static int GetOutputOrigin(ChannelId outputId) => (outputId.Value - 1) * 90 + 0x0a31;
}
