using DigiMixer.Core;
using System.Diagnostics.CodeAnalysis;

namespace DigiMixer;

public partial class Mixer
{
    private class MixerReceiver : IMixerReceiver
    {
        private readonly Mixer mixer;
        private readonly Dictionary<ChannelId, InputChannel> leftOrMonoInputChannelMap;
        private readonly Dictionary<ChannelId, InputChannel> rightInputChannelMap;
        private readonly Dictionary<ChannelId, OutputChannel> leftOrMonoOutputChannelMap;
        private readonly Dictionary<ChannelId, OutputChannel> rightOutputChannelMap;
        private readonly Dictionary<ChannelId, ChannelBase> allChannelMap;
        private readonly Dictionary<(ChannelId, ChannelId), InputOutputMapping> mappings;

        internal MixerReceiver(Mixer mixer)
        {
            this.mixer = mixer;
            leftOrMonoInputChannelMap = mixer.InputChannels.ToDictionary(ch => ch.LeftOrMonoChannelId);
            leftOrMonoOutputChannelMap = mixer.OutputChannels.ToDictionary(ch => ch.LeftOrMonoChannelId);
            rightInputChannelMap = mixer.InputChannels.Where(ch => ch.IsStereo).ToDictionary(ch => ch.RightChannelId!.Value);
            rightOutputChannelMap = mixer.OutputChannels.Where(ch => ch.IsStereo).ToDictionary(ch => ch.RightChannelId!.Value);
            allChannelMap = leftOrMonoInputChannelMap.Select(pair => KeyValuePair.Create(pair.Key, (ChannelBase) pair.Value))
                .Concat(leftOrMonoOutputChannelMap.Select(pair => KeyValuePair.Create(pair.Key, (ChannelBase) pair.Value)))
                .Concat(rightInputChannelMap.Select(pair => KeyValuePair.Create(pair.Key, (ChannelBase) pair.Value)))
                .Concat(rightOutputChannelMap.Select(pair => KeyValuePair.Create(pair.Key, (ChannelBase) pair.Value)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            // We assume that even for split faders, we're happy to only *report* via a single input/output channel pair.
            // (We will ignore any information about other combinations.)
            mappings = mixer.InputChannels
                .SelectMany(ic => ic.OutputMappings)
                .ToDictionary(om => (om.InputChannel.LeftOrMonoChannelId, om.OutputChannel.LeftOrMonoChannelId));
        }

        public void ReceiveChannelName(ChannelId channelId, string? name)
        {
            if (TryGetChannelBase(channelId, out var channel))
            {
                if (channelId == channel.LeftOrMonoChannelId)
                {
                    channel.LeftOrMonoName = name;
                }
                else
                {
                    channel.RightName = name;
                }
            }
        }

        public void ReceiveFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
        {
            if (TryGetInputOutputMapping(inputId, outputId, out var mapping))
            {
                mapping.FaderLevel = level;
            }
        }

        public void ReceiveFaderLevel(ChannelId outputId, FaderLevel level)
        {
            if (TryGetOutputChannel(outputId, out var channel))
            {
                channel.FaderLevel = level;
            }
        }

        public void ReceiveMeterLevels((ChannelId channelId, MeterLevel level)[] levels)
        {
            foreach (var (channelId, level) in levels)
            {
                if (TryGetChannelBase(channelId, out var channel))
                {
                    if (channelId == channel.LeftOrMonoChannelId)
                    {
                        channel.MeterLevel = level;
                    }
                    else
                    {
                        channel.StereoMeterLevel = level;
                    }
                }
            }
        }

        public void ReceiveMixerInfo(MixerInfo info) => mixer.MixerInfo = info;

        public void ReceiveMuteStatus(ChannelId channelId, bool muted)
        {
            if (TryGetChannelBase(channelId, out var channel))
            {
                channel.Muted = muted;
            }
        }

        private bool TryGetOutputChannel(ChannelId channelId, [NotNullWhen(true)] out OutputChannel? channel) =>
            leftOrMonoOutputChannelMap.TryGetValue(channelId, out channel);

        private bool TryGetInputOutputMapping(ChannelId inputId, ChannelId outputId, [NotNullWhen(true)] out InputOutputMapping? mapping) =>
            mappings.TryGetValue((inputId, outputId), out mapping);

        private bool TryGetChannelBase(ChannelId channelId, [NotNullWhen(true)] out ChannelBase? channel) =>
            allChannelMap.TryGetValue(channelId, out channel);
    }
}