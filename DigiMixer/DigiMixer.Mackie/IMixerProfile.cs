using DigiMixer.Core;
using DigiMixer.Mackie.Core;

namespace DigiMixer.Mackie;

internal interface IMixerProfile
{
    int GetFaderAddress(ChannelId inputId, ChannelId outputId);
    int GetFaderAddress(ChannelId outputId);
    int GetMuteAddress(ChannelId channelId);
    int GetNameAddress(ChannelId channelId);
    int GetStereoLinkAddress(ChannelId channelId);

    int InputChannelCount { get; }
    int AuxChannelCount { get; }

    internal static IMixerProfile GetProfile(MackiePacket handshakePacket)
    {
        if (handshakePacket.Body.Length != 16)
        {
            return DL16SProfile.Instance;
        }

        // TODO: Work out whether this is the right way to detect a DL32R.
        if (handshakePacket.Body.Data[15] == 3)
        {
            return DL32RProfile.Instance;
        }
        return DL16SProfile.Instance;
    }
}
