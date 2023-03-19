using DigiMixer.Core;
using System.Diagnostics.CodeAnalysis;

namespace DigiMixer;

public partial class Mixer
{
	private class MixerReceiver : IMixerReceiver
	{
		private readonly Mixer mixer;

		internal MixerReceiver(Mixer mixer)
		{
			this.mixer = mixer;
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
			mixer.leftOrMonoOutputChannelMap.TryGetValue(channelId, out channel);

		private bool TryGetInputOutputMapping(ChannelId inputId, ChannelId outputId, [NotNullWhen(true)] out InputOutputMapping? mapping) =>
			mixer.mappings.TryGetValue((inputId, outputId), out mapping);

		private bool TryGetChannelBase(ChannelId channelId, [NotNullWhen(true)] out ChannelBase? channel) =>
			mixer.allChannelMap.TryGetValue(channelId, out channel);

	}
}