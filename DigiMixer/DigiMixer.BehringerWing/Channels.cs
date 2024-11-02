using DigiMixer.Core;
using System.Collections.Immutable;

namespace DigiMixer.BehringerWing;

internal static class Channels
{
    /// <summary>
    /// The number of channels deemed "input channels" in Wing itself. This does not include aux inputs (which DigiMixer counts as input channels.)
    /// </summary>
    internal const int WingInputCount = 40;
    internal const int AuxCount = 8;
    internal const int AllInputsCount = WingInputCount + AuxCount;

    internal static ChannelId FirstWingInputChannelId { get; }  = ChannelId.Input(1);
    internal static ChannelId FirstAuxChannelId { get; } = ChannelId.Input(WingInputCount + 1);

    internal const int MainCount = 4;
    internal const int BusCount = 16;
    internal const int AllOutputsCount = MainCount + BusCount;

    internal static ChannelId FirstMainChannelId { get; } = ChannelId.Output(1);
    internal static ChannelId FirstBusChannelId { get; } = ChannelId.Output(MainCount + 1);

    // TODO: Use the above counts instead? (The hashes are generated from those anyway though...)
    internal static ImmutableList<ChannelId> AllInputChannels { get; } = [.. InputChannelHashes.AllInputs.Select(hashes => hashes.Id)];
    internal static ImmutableList<ChannelId> AllOutputChannels { get; } = [.. OutputChannelHashes.AllOutputs.Select(hashes => hashes.Id)];

    // We model the "right" channels as the "left" channels + 1000, with a hope that nothing assumes
    // they're contiguous.
    internal static ChannelId RightStereoChannel(ChannelId left) => left.WithValue(left.Value + 1000);

}
