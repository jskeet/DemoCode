using DigiMixer.Core;

namespace DigiMixer.Mackie;

/// <summary>
/// Mackie-specific information about a "notionally output" channel.
/// (This can be the main LR, an aux, or an FX output.)
/// </summary>
internal class MackieOutputChannel
{
    internal OutputGroup Group { get; }
    internal int IndexInGroup { get; }
    internal ChannelId Id { get; }

    internal int MeterAddress { get; }
    internal int? MuteAddress { get; }
    internal int? StereoLinkAddress { get; }
    internal int? NameIndex { get; }
    internal int? FaderAddress { get; }

    internal MackieOutputChannel(OutputGroup group, int indexInGroup, ChannelId id, int meterAddress, int? muteAddress, int? stereoLinkAddress, int? faderAddress, int? nameIndex)
    {
        Group = group;
        IndexInGroup = indexInGroup;
        Id = id;
        MeterAddress = meterAddress;
        MuteAddress = muteAddress;
        StereoLinkAddress = stereoLinkAddress;
        FaderAddress = faderAddress;
        NameIndex = nameIndex;
    }

    /*

    internal static MackieOutputChannel MainLeft(int meterIndex, int channelStartAddress, int muteOffset, int faderOffset, int nameIndex) =>
        new MackieOutputChannel(OutputGroup.Main, 1, ChannelId.MainOutputLeft, meterIndex, channelStartAddress, muteOffset, faderOffset, nameIndex);

    /// <summary>
    /// The main right channel, which can't be controlled or named independently from the main left channel.
    /// </summary>
    internal static MackieOutputChannel MainRight(int meterIndex) =>
        new MackieOutputChannel(OutputGroup.Main, 2, ChannelId.MainOutputRight, meterIndex, null, null, null, null);

    internal static MackieOutputChannel Fx(int index, int meterIndex, int channelStartAddress, int muteOffset, int faderOffset, int nameIndex) =>
        new MackieOutputChannel(OutputGroup.Fx, index, ChannelId.Output(index + 50), meterIndex, channelStartAddress, muteOffset, faderOffset, nameIndex);

    internal static MackieOutputChannel Aux(int index, int meterIndex, int channelStartAddress, int muteOffset, int faderOffset, int nameIndex) =>
        new MackieOutputChannel(OutputGroup.Aux, index, ChannelId.Output(index), meterIndex, channelStartAddress, muteOffset, faderOffset, nameIndex);*/

}
