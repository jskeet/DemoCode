using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using OscCore;

namespace DigiMixer.Osc;

/// <summary>
/// Factory methods for working with RCF digital mixers (M-18, M-20X)
/// </summary>
public static class Rcf
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int outboundPort = 8000, int inboundPort = 9000, MixerApiOptions? options = null) =>
        new AutoReceiveMixerApi(new RcfOscMixerApi(logger, host, outboundPort, inboundPort, options));

    public static IMixerApi CreateMixerApiForProxy(ILogger logger, string host, int proxyPort = 8001, MixerApiOptions? options = null) =>
        new AutoReceiveMixerApi(new RcfOscMixerApi(logger, host, proxyPort, options));

    // See https://github.com/Jille/rcf-m18/blob/master/gen/source.txt for more information
    // App also sends:
    // /xy/99/up_0xy (xy 00-31)
    // /00/xy/np_000 (xy 11-15)
    // /01/00/pc_000
    // /00/00/pl_000

    // TODO: Does the mixer *need* bundles, or can it work with individual messages?
    // Do we need to wrap the client in a "transmit each message in a bundle"?
    private class RcfOscMixerApi : OscMixerApiBase
    {
        private const string FirmwareAddress = "/22/00/gb_100";
        private const string TargetIdAddress = "/22/00/gb_146";
        private const string SerialNumberAddress = "/22/00/gb_275";
        private const string OptimizedMeterSendingAddress = "/22/00/gb_172";
        private const string StereoLinkAddress = "/27/00/gb_078";
        private const string RequestPollAddress = "/00/00/pl_000";

        private const string MetersAddress = "/00/00/vmeter";

        private MixerInfo currentInfo = MixerInfo.Empty;

        private static readonly string[] RequestAllInfoAddresses = Enumerable.Range(1, 13)
            .Append(22)
            .Select(i => $"/{i:00}/99/up_{i:000}")
            .Append(FirmwareAddress)
            .Append(TargetIdAddress)
            .Append(SerialNumberAddress)
            .ToArray();

        internal RcfOscMixerApi(ILogger logger, string host, int outboundPort, int inboundPort, MixerApiOptions? options) :
            base(logger, logger => new UdpOscClient(logger, host, outboundPort, inboundPort), options)
        {
        }

        internal RcfOscMixerApi(ILogger logger, string host, int proxyPort, MixerApiOptions? options) :
            base(logger, logger => new UdpOscClient(logger, host, proxyPort), options)
        {
        }

        public override async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
        {
            foreach (var address in RequestAllInfoAddresses)
            {
                await Client.SendAsync(new OscMessage(address, 0));
                // /01/99/up_01 actually takes a bit longer, but that's okay.
                // We're not relying on having all the data - we just don't want buffers to get too full.
                await Task.Delay(20);
            }
            // There's no address for an *actual* Main channel name, so we just fake it here.
            Receiver.ReceiveChannelName(ChannelId.MainOutputLeft, "Main");
        }

        public override async Task Connect(CancellationToken cancellationToken)
        {
            // TODO: We've seen this part fail in the call to CheckConnection.
            // It's unclear whether that's because we really need to send the next messages
            // before we do a connection check. It seems to recover after a while, and that's confusing.
            await base.Connect(cancellationToken);
            await Client.SendAsync(new OscMessage(OptimizedMeterSendingAddress, "1"));
            await Client.SendAsync(new OscMessage(RequestPollAddress, 1f));
        }

        public override async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
        {
            var messages = new[] { new OscMessage(StereoLinkAddress, " ") };
            var result = await InfoReceiver.RequestAndWait(Client, cancellationToken, messages, StereoLinkAddress);
            if (result is null)
            {
                throw new InvalidOperationException("Detection timed out");
            }
            // TODO: Is there such a thing as an output stereo pair?
            // (Sort of: we can assign Main L and Main R to arbitrary buses, but they're not known to be paired.
            // We could potentially detect that.)
            var independentInputs = (string) result[StereoLinkAddress][0];
            var inputChannelNumbers = independentInputs.Split('-', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var unlistedRightChannelNumbers = inputChannelNumbers
                .Where(x => (x & 1) == 1) // Start with odd numbers
                .Select(x => x + 1) // Get the corresponding even numbers
                .Where(x => !inputChannelNumbers.Contains(x)) // Exclude the ones that are already listed
                .ToList();

            // Now find all inputs - this will basically be "all of 1-20" but we might as well account for the possibility
            // of inputs being "removed".
            var inputs = inputChannelNumbers
                .Concat(unlistedRightChannelNumbers)
                .OrderBy(x => x)
                .Select(ChannelId.Input)
                .ToList();
            // Create stereo pairs based on the detected "right channels".
            var stereoPairs = unlistedRightChannelNumbers
                .OrderBy(x => x)
                .Select(x => new StereoPair(ChannelId.Input(x - 1), ChannelId.Input(x), StereoFlags.None))
                .Append(new StereoPair(ChannelId.MainOutputLeft, ChannelId.MainOutputRight, StereoFlags.None));
            var outputs = Enumerable.Range(1, 6).Select(ch => ChannelId.Output(ch))
                .Append(ChannelId.MainOutputLeft).Append(ChannelId.MainOutputRight);
            return new MixerChannelConfiguration(inputs, outputs, stereoPairs);
        }

        // TODO: Check this.
        public override TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(3);

        public override async Task SendKeepAlive()
        {
            await Client.SendAsync(new OscMessage(RequestPollAddress, 1f));
        }

        public override async Task<bool> CheckConnection(CancellationToken cancellationToken)
        {
            var result = await InfoReceiver.RequestAndWait(Client, cancellationToken, new[] { new OscMessage(FirmwareAddress, 0) }, FirmwareAddress);
            return result is not null;
        }

        protected override void PopulateReceiverMap(Dictionary<string, Action<OscMessage>> map)
        {
            map[FirmwareAddress] = message =>
            {
                currentInfo = currentInfo with { Version = (string) message[0] };
                Receiver.ReceiveMixerInfo(currentInfo);
            };

            map[TargetIdAddress] = message =>
            {
                currentInfo = currentInfo with { Model = (string) message[0] };
                Receiver.ReceiveMixerInfo(currentInfo);
            };

            map[SerialNumberAddress] = message =>
            {
                currentInfo = currentInfo with { Name = (string) message[0] };
                Receiver.ReceiveMixerInfo(currentInfo);
            };

            map[MetersAddress] = message =>
            {
                // Meter format:
                // 0-19: Inputs (20)
                // 20-25: FX returns (L+R)
                // 26-27: Main
                // 28-30: FX sends
                // 31-36: Aux
                // 37-38: Phones out (L+R)
                // 39-40: Recording (L+R)
                if (message.Count != 41)
                {
                    return;
                }

                var levels = new (ChannelId, MeterLevel)[20 + 6 + 2];
                int index = 0;

                for (int i = 1; i <= 20; i++)
                {
                    ChannelId inputId = ChannelId.Input(i);
                    levels[index++] = (inputId, ToMeterLevel((float) message[i - 1]));
                }
                for (int i = 1; i <= 6; i++)
                {
                    ChannelId outputId = ChannelId.Output(i);
                    levels[index++] = (outputId, ToMeterLevel((float) message[i + 30]));
                }
                levels[index++] = (ChannelId.MainOutputLeft, ToMeterLevel((float) message[26]));
                levels[index++] = (ChannelId.MainOutputRight, ToMeterLevel((float) message[27]));
                Receiver.ReceiveMeterLevels(levels);
            };

            static MeterLevel ToMeterLevel(float value) => new MeterLevel(value);
        }

        // Addresses

        protected override string GetFaderAddress(ChannelId inputId, ChannelId outputId) =>
            $"{GetOutputPrefix(outputId)}/{inputId.Value:00}/fd_000";

        protected override string GetFaderAddress(ChannelId outputId) =>
            outputId.IsMainOutput ? "/00/00/fd_000" : $"/26/{outputId.Value:00}/fd_000";

        // FIXME: This is just the main mute per input. We don't (apparently) have
        // linked mutes.
        protected override string GetMuteAddress(ChannelId channelId) =>
            channelId.IsInput ? $"/01/{channelId.Value:00}/mt_000"
            : channelId.IsMainOutput ? "/00/00/mt_000" : $"/26/{channelId.Value:00}/mt_000";

        protected override string GetNameAddress(ChannelId channelId) =>
            channelId.IsInput ? $"/22/00/gb_1{channelId.Value:00}"
            // FIXME: Unclear whether Main has a name address
            : channelId.IsMainOutput ? "/22/00/fixme"
            : $"/22/00/gb_12{channelId.Value:0}";

        // Note: ignoring FX returns (21-23) here, but including the player channels (19-20).
        protected override IEnumerable<ChannelId> GetPotentialInputChannels() =>
            Enumerable.Range(1, 20).Select(ChannelId.Input);
        protected override IEnumerable<ChannelId> GetPotentialOutputChannels() =>
            Enumerable.Range(1, 6).Select(ChannelId.Output).Append(ChannelId.MainOutputLeft);

        private static string GetOutputPrefix(ChannelId outputId)
        {
            int translatedValue = outputId.IsMainOutput ? 1 : outputId.Value + 4;
            return $"/{translatedValue:00}";
        }

        protected override object MutedValue { get; } = 1f;
        protected override object UnmutedValue { get; } = 0f;
    }
}
