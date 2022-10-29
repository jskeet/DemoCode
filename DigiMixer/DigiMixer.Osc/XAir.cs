// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Core;
using DigiMixer.Osc;
using Microsoft.Extensions.Logging;
using OscCore;

namespace OscMixerControl;

/// <summary>
/// Factory methods and constants for working with X-Air mixers
/// (e.g. XR12, XR16, XR18).
/// </summary>
public static class XAir
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 10024) =>
        new XAirOscMixerApi(logger, host, port);

    // TODO: Should the properties be exposed anywhere?

    /// <summary>
    /// The output channel ID for the left side of the main output.
    /// </summary>
    private static ChannelId MainOutputLeft { get; } = new ChannelId(100, false);

    /// <summary>
    /// The output channel ID for the right side of the main output.
    /// </summary>
    private static ChannelId MainOutputRight { get; } = new ChannelId(101, false);

    /// <summary>
    /// The input channel ID for the left "aux" input.
    /// </summary>
    private static ChannelId AuxInputLeft { get; } = new ChannelId(17, input: true);

    /// <summary>
    /// The input channel ID for the right "aux" input.
    /// </summary>
    private static ChannelId AuxInputRight { get; } = new ChannelId(18, input: true);

    private class XAirOscMixerApi : OscMixerApiBase
    {
        private const string XRemoteAddress = "/xremote";
        private const string InfoAddress = "/info";
        private const string InputChannelLevelsMeter = "/meters/1";
        private const string OutputChannelLevelsMeter = "/meters/5";
        private const string InputChannelLinkAddress = "/config/chlink";
        private const string BusChannelLinkAddress = "/config/buslink";

        internal XAirOscMixerApi(ILogger logger, string host, int port) : base(logger, logger => new UdpOscClient(logger, host, port))
        {
        }

        public override async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
        {
            await Client.SendAsync(new OscMessage(InfoAddress));
            // TODO: Apply some rigour to the delays - potentially wait for responses using InfoReceiver?
            foreach (var channelId in channelIds)
            {
                await Client.SendAsync(new OscMessage(GetMuteAddress(channelId)));
                await Client.SendAsync(new OscMessage(GetNameAddress(channelId)));
                if (channelId.IsOutput)
                {
                    await Client.SendAsync(new OscMessage(GetFaderAddress(channelId)));
                }
                await Task.Delay(20);
            }

            foreach (var input in channelIds.Where(c => c.IsInput))
            {
                foreach (var output in channelIds.Where(c => c.IsOutput))
                {
                    await Client.SendAsync(new OscMessage(GetFaderAddress(input, output)));
                }
                await Task.Delay(20);
            }
        }

        public override async Task<MixerChannelConfiguration> DetectConfiguration()
        {
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
            var result = await InfoReceiver.RequestAndWait(Client, token,
                InfoAddress,
                InputChannelLinkAddress,
                BusChannelLinkAddress);
            var inputLinks = result[InputChannelLinkAddress].Select(x => x is 1).ToList();
            var outputLinks = result[BusChannelLinkAddress].Select(x => x is 1).ToList();
            var model = (string) result[InfoAddress][2];

            (int inputCount, int outputCount) = model switch
            {
                "XR12" => (12, 2),
                "XR16" => (16, 4),
                "XR18" => (16, 6),
                _ => (inputLinks.Count * 2, outputLinks.Count * 2)
            };
            var inputs = Enumerable.Range(1, inputCount).Select(i => new ChannelId(i, input: true));
            if (model == "XR18")
            {
                inputs = inputs.Append(XAir.AuxInputLeft).Append(XAir.AuxInputRight);
            }
            var outputs = Enumerable.Range(1, outputCount).Select(i => new ChannelId(i, input: false))
                .Append(XAir.MainOutputLeft).Append(XAir.MainOutputRight);

            var stereoPairs = CreateStereoPairs(inputCount, inputLinks, input: true)
                .Concat(CreateStereoPairs(outputCount, outputLinks, input: false))
                .Append(new StereoPair(XAir.MainOutputLeft, XAir.MainOutputRight, StereoFlags.None));
            return new MixerChannelConfiguration(inputs, outputs, stereoPairs);

            IEnumerable<StereoPair> CreateStereoPairs(int max, List<bool> pairs, bool input)
            {
                var count = Math.Min(max, pairs.Count * 2);
                for (int i = 1; i <= count - 1; i += 2)
                {
                    if (pairs[i / 2])
                    {
                        yield return new StereoPair(new ChannelId(i, input), new ChannelId(i + 1, input), StereoFlags.SplitNames);
                    }
                }
            }
        }

        public override async Task SendKeepAlive()
        {
            await Client.SendAsync(new OscMessage(XRemoteAddress));
            await Client.SendAsync(new OscMessage("/batchsubscribe", InputChannelLevelsMeter, InputChannelLevelsMeter, 0, 0, 0 /* fast */));
            await Client.SendAsync(new OscMessage("/batchsubscribe", OutputChannelLevelsMeter, OutputChannelLevelsMeter, 0, 0, 0 /* fast */));
        }

        protected override void PopulateReceiverMap(Dictionary<string, Action<IMixerReceiver, OscMessage>> map)
        {
            map[InfoAddress] = (receiver, message) =>
            {
                string version = (string) message[0] + " / " + (string) message[3];
                string model = (string) message[2];
                string name = (string) message[1];
                receiver.ReceiveMixerInfo(new MixerInfo(model, name, version));
            };

            map[InputChannelLevelsMeter] = (receiver, message) =>
            {
                var blob = (byte[]) message[0];
                for (int i = 1; i <= 18; i++)
                {
                    ChannelId inputId = new ChannelId(i, input: true);
                    receiver.ReceiveMeterLevel(inputId, ToMeterLevel(blob, i - 1));
                }
            };

            map[OutputChannelLevelsMeter] = (receiver, message) =>
            {
                var blob = (byte[]) message[0];
                for (int i = 1; i <= 6; i++)
                {
                    ChannelId outputId = new ChannelId(i, input: false);
                    receiver.ReceiveMeterLevel(outputId, ToMeterLevel(blob, i - 1));
                }
                receiver.ReceiveMeterLevel(XAir.MainOutputLeft, ToMeterLevel(blob, 6));
                receiver.ReceiveMeterLevel(XAir.MainOutputRight, ToMeterLevel(blob, 7));
            };

            static MeterLevel ToMeterLevel(byte[] blob, int index)
            {
                short level = BitConverter.ToInt16(blob, index * 2 + 4);
                return MeterLevel.FromDb(level / 256.0);
            }
        }

        // Addresses

        protected override string GetFaderAddress(ChannelId inputId, ChannelId outputId)
        {
            string prefix = GetInputPrefix(inputId);
            return prefix + (outputId == MainOutputLeft ? "/mix/fader" : $"/mix/{outputId.Value:00}/level");
        }

        protected override object MutedValue { get; } = 0;
        protected override object UnmutedValue { get; } = 1;
        protected override string GetFaderAddress(ChannelId outputId) => GetOutputPrefix(outputId) + "/mix/fader";
        protected override string GetMuteAddress(ChannelId channelId) => GetPrefix(channelId) + "/mix/on";
        protected override string GetNameAddress(ChannelId channelId) => GetPrefix(channelId) + "/config/name";
        protected override IEnumerable<ChannelId> GetPotentialInputChannels() => Enumerable.Range(1, 16).Select(id => new ChannelId(id, input: true)).Append(AuxInputLeft);
        protected override IEnumerable<ChannelId> GetPotentialOutputChannels() => Enumerable.Range(1, 6).Select(id => new ChannelId(id, input: false)).Append(MainOutputLeft);

        private static string GetInputPrefix(ChannelId inputId) =>
            inputId == AuxInputLeft ? "/rtn/aux" : $"/ch/{inputId.Value:00}";

        private static string GetOutputPrefix(ChannelId outputId) =>
            outputId == MainOutputLeft ? "/lr" : $"/bus/{outputId.Value}";

        private static string GetPrefix(ChannelId channelId) =>
            channelId.IsInput ? GetInputPrefix(channelId) : GetOutputPrefix(channelId);
    }
}
