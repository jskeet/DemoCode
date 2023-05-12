using DigiMixer.Core;

namespace DigiMixer.Mackie;

internal class NullProfile : IMixerProfile
{
    internal static NullProfile Instance { get; } = new NullProfile();

    private NullProfile()
    {
    }

    public int InputChannelCount => 0;

    public int AuxChannelCount => 0;

    public int GetFaderAddress(ChannelId inputId, ChannelId outputId) => -1;
    public int GetFaderAddress(ChannelId outputId) => -1;
    public int GetMuteAddress(ChannelId channelId) => -1;
    public int GetNameAddress(ChannelId channelId) => -1;
    public int GetStereoLinkAddress(ChannelId channelId) => -1;
}
