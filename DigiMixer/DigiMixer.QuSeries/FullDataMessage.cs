using DigiMixer.Core;
using DigiMixer.QuSeries.Core;
using System.Runtime.InteropServices;
using System.Text;

namespace DigiMixer.QuSeries;

using static QuConversions;

/// <summary>
/// A wrapper around the "full data" response, providing access to semantic aspects
/// of the response.
/// </summary>
internal sealed class FullDataMessage
{

    // Layout:
    // 0x30 bytes of unknown meaning
    // Channels, 0xc0 bytes each:
    // - 32 input channels (0-31)
    // - 3 stereo channels (32-34)
    // - 4 FX ret channels (35-38)
    // - 7 mix channels (39-45 = 0x27-0x2d)
    // - Main output (46 = 0x2e)
    // - 4 group channels (47-50)
    // - 4 FX send channels (55-58)

    // These channel IDs are used elsewhere as well...

    // Within each 0xc0 bytes - input channels, at least...

    // 0x1a: PEQ (1 byte)
    // 0x29: Compressor (1 byte)
    // 0x34: Gate (1 byte)
    // 0x7e: Fader (2 bytes)
    // 0x85: 48v (1 byte)
    // 0x86: Polarity (1 byte)
    // 0x87: Routing (1 byte)
    // 0x88: Mute (1 byte)
    // 0x90: Stereo link (1 byte)
    // 0x9c: Name (6 bytes)
    // 0xaf: Ducker threshold (???)

    // Channel fader levels:
    // Per channel blocks of 0xa0 bytes each, 8 bytes per mix (not sure why) - gives space for 20 mixes
    // Channel 1, mix 1 is at 0x2e60
    // Channel 1, mix 2 is at 0x2e68 etc

    // Channel 2, mix 1 is at 0x2f00

    private readonly QuGeneralMessage message;

    // TODO: Try to detect these. (Would need a Qu-16 or similar to validate...)
    public int InputCount => 32;
    public int MonoMixChannels => 4;
    public int StereoMixChannels => 6;
    public int StereoGroupChannels => 8;

    internal FullDataMessage(QuGeneralMessage message)
    {
        this.message = message;
    }

    public string? GetInputName(int channel) => GetName(InputChannelData(channel));
    public string? GetMixName(int mix) => GetName(MixChannelData(mix));
    public string? GetGroupName(int group) => GetName(GroupChannelData(group));
    public string? GetMainName() => GetName(MainChannelData());

    public bool InputMuted(int channel) => Muted(InputChannelData(channel));
    public bool MainMuted() => Muted(MainChannelData());
    public bool MixMuted(int mix) => Muted(MixChannelData(mix));
    public bool GroupMuted(int group) => Muted(GroupChannelData(group));

    public FaderLevel InputFaderLevel(int channel) => GetFaderLevel(InputChannelData(channel));
    public FaderLevel InputMixFaderLevel(int channel, int mix)
    {
        int channelOffset = 0x2e60 + (0xa0 * (channel - 1));
        int mixOffset = mix <= MonoMixChannels ? mix : (mix + 1 - MonoMixChannels) / 2 + MonoMixChannels;
        int offset = channelOffset + mixOffset * 8 - 8;
        return RawToFaderLevel(MemoryMarshal.Cast<byte, ushort>(message.Data.Slice(offset, 2))[0]);
    }

    public FaderLevel InputGroupFaderLevel(int channel, int group)
    {
        int channelOffset = 0x2e60 + (0xa0 * (channel - 1));
        // byte[] bytes = message.Data.Slice(channelOffset, 0xa0).ToArray();
        int groupOffset = (group + 1) / 2; // Map 1-8 to 1-4
        // Offset by 64 bytes for the regular mixes
        int offset = channelOffset + (8 * 8) + groupOffset * 8 - 8;
        return RawToFaderLevel(MemoryMarshal.Cast<byte, ushort>(message.Data.Slice(offset, 2))[0]);
    }

    public FaderLevel MainFaderLevel() => GetFaderLevel(MainChannelData());
    public FaderLevel MixFaderLevel(int mix) => GetFaderLevel(MixChannelData(mix));
    public FaderLevel GroupFaderLevel(int group) => GetFaderLevel(GroupChannelData(group));

    private FaderLevel GetFaderLevel(ReadOnlySpan<byte> channelData) =>
        RawToFaderLevel(MemoryMarshal.Cast<byte, ushort>(channelData.Slice(0x7e, 2))[0]);

    private string? GetName(ReadOnlySpan<byte> channelData)
    {
        string name = Encoding.ASCII.GetString(channelData.Slice(0x9c, 6)).TrimEnd('\0');
        return name == "" ? null : name;
    }
    private bool Muted(ReadOnlySpan<byte> channelData) => channelData[0x88] == 1;

    private ReadOnlySpan<byte> InputChannelData(int channel) => ChannelData(channel - 1);
    private ReadOnlySpan<byte> MixChannelData(int mix) => mix <= MonoMixChannels ? ChannelData(mix + 38) : ChannelData((mix - 1) / 2 + 41);
    private ReadOnlySpan<byte> GroupChannelData(int group) => ChannelData((group - 1) / 2 + 47);
    private ReadOnlySpan<byte> MainChannelData() => ChannelData(46);

    private ReadOnlySpan<byte> ChannelData(int channel) => message.Data.Slice(0x30 + 0xc0 * channel, 0xc0);

    internal MixerChannelConfiguration CreateChannelConfiguration()
    {
        var inputs = Enumerable.Range(1, InputCount).Select(ChannelId.Input);
        var outputs = Enumerable.Range(1, MonoMixChannels + StereoMixChannels).Select(ChannelId.Output)
            .Concat(Enumerable.Range(1, StereoGroupChannels).Select(GroupChannelId))
            .Append(ChannelId.MainOutputLeft).Append(ChannelId.MainOutputRight);

        var stereoPairs = new List<StereoPair>();
        for (int i = 1; i <= InputCount; i += 2)
        {
            if (InputLinked(i))
            {
                stereoPairs.Add(new StereoPair(ChannelId.Input(i), ChannelId.Input(i + 1), StereoFlags.FullyIndependent));
            }
        }
        for (int i = 1; i <= StereoMixChannels; i += 2)
        {
            stereoPairs.Add(new StereoPair(ChannelId.Output(i + MonoMixChannels), ChannelId.Output(i + MonoMixChannels + 1), StereoFlags.None));
        }
        for (int i = 1; i <= StereoGroupChannels; i += 2)
        {
            stereoPairs.Add(new StereoPair(GroupChannelId(i), GroupChannelId(i + 1), StereoFlags.None));
        }
        stereoPairs.Add(new StereoPair(ChannelId.MainOutputLeft, ChannelId.MainOutputRight, StereoFlags.None));
        return new MixerChannelConfiguration(inputs, outputs, stereoPairs);

        bool InputLinked(int channel) => InputChannelData(channel)[0x90] == 1;
    }
}
