using DigiMixer.Core;

namespace DigiMixer.Mackie;

/// <summary>
/// Mackie-specific information about a "notionally input" channel.
/// (This can be a real input, a return, or an FX input.)
/// </summary>
internal class MackieInputChannel
{
    internal ChannelId Id { get; }
    internal int? StereoLinkAddress { get; }
    internal int MuteAddress { get; }
    internal int NameIndex { get; }
    internal int MeterAddress { get; }

    private int MainFaderAddress { get; }
    private int Aux1FaderAddress { get; }
    private int? Fx1FaderAddress { get; }

    internal MackieInputChannel(ChannelId channelId, int meterAddress, int nameIndex,
        int muteAddress, int? stereoLinkAddress, int mainFaderAddress, int aux1FaderAddress, int? fx1FaderAddress)
    {
        Id = channelId;
        MeterAddress = meterAddress;
        NameIndex = nameIndex;
        MuteAddress = muteAddress;
        StereoLinkAddress = stereoLinkAddress;
        MainFaderAddress = mainFaderAddress;
        Aux1FaderAddress = aux1FaderAddress;
        Fx1FaderAddress = fx1FaderAddress;
    }

    internal int? GetFaderAddress(MackieOutputChannel output) => output.Group switch
    {
        OutputGroup.Main => output.IndexInGroup == 1 ? MainFaderAddress : null,
        OutputGroup.Aux => Aux1FaderAddress + (output.IndexInGroup - 1) * 3,
        OutputGroup.Fx => Fx1FaderAddress + (output.IndexInGroup - 1) * 2,
        _ => null
    };
}
