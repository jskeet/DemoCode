using DigiMixer.Core;
using System.Collections.Immutable;

namespace DigiMixer.BehringerWing;

internal interface IChannelHashes
{
    ChannelId Id { get; }
    uint Name { get; }
    uint Fader { get; }
    uint Mute { get; }
}

/// <summary>
/// The node hashes associated with an input channel.
/// </summary>
internal partial record InputChannelHashes(ChannelId Id, uint Name, uint Fader, uint Mute, uint StereoMode, ImmutableList<uint> OutputLevels) : IChannelHashes;

/// <summary>
/// The node hashes associated with an output channel.
/// </summary>
internal partial record OutputChannelHashes(ChannelId Id, uint Name, uint Fader, uint Mute) : IChannelHashes;

internal static class Hashes
{
    internal static ImmutableList<IChannelHashes> AllChannelHashes { get; } =
    [.. InputChannelHashes.AllInputs.Concat<IChannelHashes>(OutputChannelHashes.AllOutputs)];

    internal static ImmutableDictionary<ChannelId, IChannelHashes> AllChannelHashesByChannelId { get; } =
        AllChannelHashes.ToImmutableDictionary(ch => ch.Id);

    internal const uint ConsoleModel = 2555849122;    // $syscfg.cnsmdl - e.g. "wing-rack"
    internal const uint FirmwareVersion = 2657714525; // $syscfg.$firmware - e.g. "3.0.4-0-g5f8d3e8f:release"
    internal const uint ConsoleName = 3385532640;     // $syscfg.consolename - e.g. "ShedWing"
}