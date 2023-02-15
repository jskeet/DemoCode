// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using OscCore;

namespace DigiMixer.Osc;

/// <summary>
/// Factory methods and constants for working with X-32 mixers.
/// </summary>
public static class X32
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 10023) =>
        new AutoReceiveMixerApi(new X32OscMixerApi(logger, host, port));

    private class X32OscMixerApi : XSeriesMixerApiBase
    {
        private const string InputChannelLinkAddressPrefix = "/config/chlink/";
        private const string BusChannelLinkAddressPrefix = "/config/buslink/";

        private static IEnumerable<string> InputChannelLinkAddresses => Enumerable.Range(1, 16)
            .Select(x => $"{InputChannelLinkAddressPrefix}{x * 2 - 1}-{x * 2}");

        private static IEnumerable<string> BusChannelLinkAddresses => Enumerable.Range(1, 8)
            .Select(x => $"{BusChannelLinkAddressPrefix}{x * 2 - 1}-{x * 2}");

        internal X32OscMixerApi(ILogger logger, string host, int port) : base(logger, host, port)
        {
        }

        protected override string InputChannelLevelsMeter { get; } = "/meters/1";
        protected override string OutputChannelLevelsMeter { get; } = "/meters/2";

        public override async Task<MixerChannelConfiguration> DetectConfiguration()
        {
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
            var result = await InfoReceiver.RequestAndWait(Client, token,
                InputChannelLinkAddresses.Concat(BusChannelLinkAddresses).ToArray());
            if (result is null)
            {
                throw new InvalidOperationException("Detection timed out");
            }
            var inputs = Enumerable.Range(1, 32).Select(i => ChannelId.Input(i));
            var outputs = Enumerable.Range(1, 16).Select(i => ChannelId.Output(i))
                .Append(MainOutputLeft).Append(MainOutputRight);
            var stereoPairs = CreateStereoPairs(32, InputChannelLinkAddressPrefix, ChannelId.Input)
                .Concat(CreateStereoPairs(16, BusChannelLinkAddressPrefix, ChannelId.Output))
                .Append(new StereoPair(MainOutputLeft, MainOutputRight, StereoFlags.None));
            return new MixerChannelConfiguration(inputs, outputs, stereoPairs);

            IEnumerable<StereoPair> CreateStereoPairs(int totalChannels, string prefix, Func<int, ChannelId> factory)
            {
                for (int i = 1; i <= totalChannels; i += 2)
                {
                    string address = $"{prefix}{i}-{i + 1}";
                    if (result[address][0] is 1)
                    {
                        yield return new StereoPair(factory(i), factory(i + 1), StereoFlags.SplitNames);
                    }
                }
            }
        }

        protected override void ReceiveInputMeters(OscMessage message)
        {
            var levels = new (ChannelId, MeterLevel)[32];
            var blob = (byte[]) message[0];
            for (int i = 1; i <= 32; i++)
            {
                ChannelId inputId = ChannelId.Input(i);
                levels[i - 1] = (inputId, ToMeterLevel(blob, i - 1));
            }
            Receiver.ReceiveMeterLevels(levels);
        }

        protected override void ReceiveOutputMeters(OscMessage message)
        {
            var levels = new (ChannelId, MeterLevel)[18];
            var blob = (byte[]) message[0];
            for (int i = 1; i <= 16; i++)
            {
                ChannelId outputId = ChannelId.Output(i);
                levels[i - 1] = (outputId, ToMeterLevel(blob, i - 1));
            }
            levels[16] = (MainOutputLeft, ToMeterLevel(blob, 22));
            levels[17] = (MainOutputRight, ToMeterLevel(blob, 23));
            Receiver.ReceiveMeterLevels(levels);
        }

        private static MeterLevel ToMeterLevel(byte[] blob, int index)
        {
            float raw = BitConverter.ToSingle(blob, index * 4 + 4);
            var db = raw == 0 ? double.NegativeInfinity : 8.67 * Math.Log(raw);
            return MeterLevel.FromDb(db);
        }

        // Addresses

        protected override IEnumerable<ChannelId> GetPotentialInputChannels() => Enumerable.Range(1, 32).Select(ChannelId.Input);
        protected override IEnumerable<ChannelId> GetPotentialOutputChannels() => Enumerable.Range(1, 16).Select(ChannelId.Output).Append(MainOutputLeft);

        protected override string GetInputPrefix(ChannelId inputId) =>
            $"/ch/{inputId.Value:00}";

        protected override string GetOutputPrefix(ChannelId outputId) =>
            IsMainOutput(outputId) ? "/main/st" : $"/bus/{outputId.Value:00}";
    }
}
