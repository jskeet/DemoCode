// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Core;
using DigiMixer.Osc;
using Microsoft.Extensions.Logging;
using OscCore;
using System.Diagnostics;
using System.Threading.Channels;

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
    private static ChannelId MainOutputLeft { get; } = ChannelId.Output(100);

    /// <summary>
    /// The output channel ID for the right side of the main output.
    /// </summary>
    private static ChannelId MainOutputRight { get; } = ChannelId.Output(101);

    /// <summary>
    /// The input channel ID for the left "aux" input.
    /// </summary>
    private static ChannelId AuxInputLeft { get; } = ChannelId.Input(17);

    /// <summary>
    /// The input channel ID for the right "aux" input.
    /// </summary>
    private static ChannelId AuxInputRight { get; } = ChannelId.Input(18);

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
            var stopwatch = Stopwatch.StartNew();
            await Client.SendAsync(new OscMessage(InfoAddress));
            // TODO: Apply some rigour to the delays - potentially wait for responses using InfoReceiver?
            foreach (var channelId in channelIds)
            {
                var muteAddress = GetMuteAddress(channelId);
                var nameAddress = GetNameAddress(channelId);
                var faderAddress = channelId.IsOutput ? GetFaderAddress(channelId) : null;
                var result = await InfoReceiver.RequestAndWait(Client, CreateCancellationToken(),
                    channelId.IsOutput ? new[] { muteAddress, nameAddress, GetFaderAddress(channelId) }
                    : new[] { muteAddress, nameAddress });
                if (result is null)
                {
                    Logger.LogTrace("Fetching name/mute info for {channel} timed out", channelId);
                }
            }

            foreach (var input in channelIds.Where(c => c.IsInput))
            {
                var addresses = channelIds.Where(c => c.IsOutput).Select(output => GetFaderAddress(input, output)).ToArray();
                var result = await InfoReceiver.RequestAndWait(Client, CreateCancellationToken(), addresses);
                if (result is null)
                {
                    Logger.LogTrace("Fetching fader input/output info for {channel} timed out", input);
                }
            }
            stopwatch.Stop();
            Logger.LogTrace("Requested all data in {ms}ms", stopwatch.ElapsedMilliseconds);

            // In reality we get through all of these in about 50ms, but let's allow for a glitchy connection.
            CancellationToken CreateCancellationToken() => new CancellationTokenSource(TimeSpan.FromMilliseconds(250)).Token;
        }

        public override async Task<MixerChannelConfiguration> DetectConfiguration()
        {
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
            var result = await InfoReceiver.RequestAndWait(Client, token,
                InfoAddress,
                InputChannelLinkAddress,
                BusChannelLinkAddress);
            if (result is null)
            {
                throw new InvalidOperationException("Detection timed out");
            }
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
            var inputs = Enumerable.Range(1, inputCount).Select(i => ChannelId.Input(i));
            if (model == "XR18")
            {
                inputs = inputs.Append(AuxInputLeft).Append(AuxInputRight);
            }
            var outputs = Enumerable.Range(1, outputCount).Select(i => ChannelId.Output(i))
                .Append(MainOutputLeft).Append(MainOutputRight);

            var stereoPairs = CreateStereoPairs(inputCount, inputLinks, ChannelId.Input)
                .Concat(CreateStereoPairs(outputCount, outputLinks, ChannelId.Output))
                .Append(new StereoPair(MainOutputLeft, MainOutputRight, StereoFlags.None));
            return new MixerChannelConfiguration(inputs, outputs, stereoPairs);

            IEnumerable<StereoPair> CreateStereoPairs(int max, List<bool> pairs, Func<int, ChannelId> factory)
            {
                var count = Math.Min(max, pairs.Count * 2);
                for (int i = 1; i <= count - 1; i += 2)
                {
                    if (pairs[i / 2])
                    {
                        yield return new StereoPair(factory(i), factory(i + 1), StereoFlags.SplitNames);
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
                var levels = new (ChannelId, MeterLevel)[18];
                var blob = (byte[]) message[0];
                for (int i = 1; i <= 18; i++)
                {
                    ChannelId inputId = ChannelId.Input(i);
                    levels[i - 1] = (inputId, ToMeterLevel(blob, i - 1));
                }
                receiver.ReceiveMeterLevels(levels);
            };

            map[OutputChannelLevelsMeter] = (receiver, message) =>
            {
                var levels = new (ChannelId, MeterLevel)[8];
                var blob = (byte[]) message[0];
                for (int i = 1; i <= 6; i++)
                {
                    ChannelId outputId = ChannelId.Output(i);
                    levels[i - 1] = (outputId, ToMeterLevel(blob, i - 1));
                }
                levels[6] = (MainOutputLeft, ToMeterLevel(blob, 6));
                levels[7] = (MainOutputRight, ToMeterLevel(blob, 7));
                receiver.ReceiveMeterLevels(levels);
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
            return prefix + (IsMainOutput(outputId) ? "/mix/fader" : $"/mix/{outputId.Value:00}/level");
        }

        protected override object MutedValue { get; } = 0;
        protected override object UnmutedValue { get; } = 1;
        protected override string GetFaderAddress(ChannelId outputId) => GetOutputPrefix(outputId) + "/mix/fader";
        protected override string GetMuteAddress(ChannelId channelId) => GetPrefix(channelId) + "/mix/on";
        protected override string GetNameAddress(ChannelId channelId) => GetPrefix(channelId) + "/config/name";
        protected override IEnumerable<ChannelId> GetPotentialInputChannels() => Enumerable.Range(1, 16).Select(id => ChannelId.Input(id)).Append(AuxInputLeft);
        protected override IEnumerable<ChannelId> GetPotentialOutputChannels() => Enumerable.Range(1, 6).Select(id => ChannelId.Output(id)).Append(MainOutputLeft);

        private static string GetInputPrefix(ChannelId inputId) =>
            IsAuxInput(inputId) ? "/rtn/aux" : $"/ch/{inputId.Value:00}";

        private static string GetOutputPrefix(ChannelId outputId) =>
            IsMainOutput(outputId) ? "/lr" : $"/bus/{outputId.Value}";

        private static string GetPrefix(ChannelId channelId) =>
            channelId.IsInput ? GetInputPrefix(channelId) : GetOutputPrefix(channelId);

        private static bool IsMainOutput(ChannelId channelId) =>
            channelId == MainOutputLeft || channelId == MainOutputRight;

        private static bool IsAuxInput(ChannelId channelId) =>
            channelId == AuxInputLeft || channelId == AuxInputRight;
    }
}
