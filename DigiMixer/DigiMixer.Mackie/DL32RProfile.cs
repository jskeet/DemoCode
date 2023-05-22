using DigiMixer.Core;

namespace DigiMixer.Mackie;

internal class DL32RProfile : IMixerProfile
{
    internal static DL32RProfile Instance { get; } = new DL32RProfile();

    private DL32RProfile()
    {
    }

    public int InputChannelCount => 36;
    public int AuxChannelCount => 14;
    public byte ModelNameInfoRequest => 7;

    // Known addresses (decimal):
    // Mute input 1: 48
    // Mute input 2: 180
    // Main fader input 1: 49
    // Main fader input 2: 181
    // Mute aux 1: 6133
    // Mute aux 2: 6253
    // Fader aux 1: 6134
    // Fader aux 2: 6254
    // Mute LR: 5996

    // Inputs *might* start at 41 - or possibly 39? (1000/2000, then at 173 we have 1001/2001 etc).
    // Current code assumes 41.
    // - That would put "first address past inputs" as 4265.



    // Names: (check routing)
    // 1-32: Inputs 1-32
    // 33: "From Zoom"?
    // 34:
    // 35: Clavinova
    // 36: Dante 2
    // 45+46: Line 1-2 out
    // 49: To zoom
    // 50: 
    // 51: LR
    // 52-77: ??
    // 78: MORNING SERVICE
    // 79: EVENING SERVICE
    // 82: Wedding
    // 83: Shing

    // Another ChannelNames - how do we differentiate? (Check first chunk?)
    // 1: FOH
    // 3: MON
    // 5: 3
    // 7: 4
    // 9: Song1
    // 10: 1
    // 11: 1

    public int GetFaderAddress(ChannelId inputId, ChannelId outputId)
    {
        int inputBase = GetInputOrigin(inputId);
        int offset = outputId.IsMainOutput ? 8 : outputId.Value * 3 + 55;
        return inputBase + offset;
    }

    public int GetFaderAddress(ChannelId outputId) =>
        outputId.IsMainOutput ? 5997 : GetOutputOrigin(outputId) + 1;

    public int GetMuteAddress(ChannelId channelId) =>
        channelId.IsInput ? GetInputOrigin(channelId) + 0x07
        : channelId.IsMainOutput ? 5996
        : GetOutputOrigin(channelId) + 0;

    public int GetNameAddress(ChannelId channelId) =>
        channelId.IsInput ? channelId.Value
        : channelId.IsMainOutput ? 51
        : channelId.Value + 51;

    public int GetStereoLinkAddress(ChannelId channelId) =>
        channelId.IsInput
        ? GetInputOrigin(channelId) + 0x0b
        : GetOutputOrigin(channelId) + 0x04;

    private static int GetInputOrigin(ChannelId inputId) => (inputId.Value - 1) * 132 + 41;
    private static int GetOutputOrigin(ChannelId outputId) => (outputId.Value - 1) * 120 + 6133;
}
