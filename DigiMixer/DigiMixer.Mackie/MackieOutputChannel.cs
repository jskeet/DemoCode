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
}
