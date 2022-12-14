using DigiMixer.Core;

namespace DigiMixer;

internal static class MixerChannelConfigurationExtensions
{
    internal static IReadOnlyList<MonoOrStereoPairChannelId> GetPossiblyPairedInputs(this MixerChannelConfiguration configuration)
    {
        var leftStereos = configuration.StereoPairs.ToDictionary(pair => pair.Left);
        var rightStereos = configuration.StereoPairs.Select(pair => pair.Right).ToHashSet();

        return configuration.InputChannels.Select(GetPair)
            .OfType<MonoOrStereoPairChannelId>()
            .ToList()
            .AsReadOnly();

        MonoOrStereoPairChannelId? GetPair(ChannelId channelId) =>
            leftStereos.TryGetValue(channelId, out var pair) ? new MonoOrStereoPairChannelId(channelId, pair.Right, pair.Flags)
            : rightStereos.Contains(channelId) ? null
            : new MonoOrStereoPairChannelId(channelId, null, StereoFlags.None);
    }

    internal static IReadOnlyList<MonoOrStereoPairChannelId> GetPossiblyPairedOutputs(this MixerChannelConfiguration configuration)
    {
        var leftStereos = configuration.StereoPairs.ToDictionary(pair => pair.Left);
        var rightStereos = configuration.StereoPairs.Select(pair => pair.Right).ToHashSet();
        return configuration.OutputChannels.Select(GetPair)
                .OfType<MonoOrStereoPairChannelId>()
                .ToList()
                .AsReadOnly();
        MonoOrStereoPairChannelId? GetPair(ChannelId channelId) =>
            leftStereos.TryGetValue(channelId, out var pair) ? new MonoOrStereoPairChannelId(channelId, pair.Right, pair.Flags)
            : rightStereos.Contains(channelId) ? null
            : new MonoOrStereoPairChannelId(channelId, null, StereoFlags.None);
    }
}
