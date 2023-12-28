// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using OscCore;
using System.Buffers.Binary;

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

        private static readonly string[] outputMappingAddresses = Enumerable.Range(1, 16)
            .Select(GetOutputMappingAddress)
            .ToArray();

        private static IEnumerable<string> InputChannelLinkAddresses => Enumerable.Range(1, 16)
            .Select(x => $"{InputChannelLinkAddressPrefix}{x * 2 - 1}-{x * 2}");

        private static IEnumerable<string> BusChannelLinkAddresses => Enumerable.Range(1, 8)
            .Select(x => $"{BusChannelLinkAddressPrefix}{x * 2 - 1}-{x * 2}");

        // 17 values (so we don't need to subtract 1 all the time):
        // For each element:
        // 0 = Unknown
        // 1-16 = Output channels 1-16
        // 100/101 = Main L/R
        private int[] mainOutputToChannelId;

        internal X32OscMixerApi(ILogger logger, string host, int port) : base(logger, host, port)
        {
            mainOutputToChannelId = new int[17];
        }

        // Meter 1 gives input pre-fader/mute, so it's easy to tell what the "raw"
        // input is after the pre-amp - so it's worth adjusting the pre-amp gain.
        protected override string InputChannelLevelsMeter { get; } = "/meters/1";
        // Meter 4 gives the equivalent of the Meters => In/Out page, which is post-fader.
        // This is most useful in terms of knowing what's actually being sent.
        // (It *is* affected by mute; not sure if this is good or not really.)
        protected override string OutputChannelLevelsMeter { get; } = "/meters/4";

        public override async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
        {
            var result = await InfoReceiver.RequestAndWait(Client, cancellationToken,
                InputChannelLinkAddresses.Concat(BusChannelLinkAddresses).ToArray());
            if (result is null)
            {
                throw new InvalidOperationException("Detection timed out");
            }
            var inputs = Enumerable.Range(1, 32).Select(i => ChannelId.Input(i));
            var outputs = Enumerable.Range(1, 16).Select(i => ChannelId.Output(i))
                .Append(ChannelId.MainOutputLeft).Append(ChannelId.MainOutputRight);
            var stereoPairs = CreateStereoPairs(32, InputChannelLinkAddressPrefix, ChannelId.Input)
                .Concat(CreateStereoPairs(16, BusChannelLinkAddressPrefix, ChannelId.Output))
                .Append(new StereoPair(ChannelId.MainOutputLeft, ChannelId.MainOutputRight, StereoFlags.None));
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

        protected override async Task RequestAdditionalData()
        {
            var result = await InfoReceiver.RequestAndWait(Client, CreateCancellationToken(), outputMappingAddresses);
            if (result is null)
            {
                Logger.LogTrace("Fetching output mapping failed");
            }
            CancellationToken CreateCancellationToken() => new CancellationTokenSource(TimeSpan.FromMilliseconds(250)).Token;
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
            // /meters/2 has lots of information: we just look at the outputs,
            // expecting all of our output channel IDs to be mapped to one of
            // the main outputs. (We don't currently use the aux outputs etc.)
            // When all the original data is requested, we map outputs to channel IDs,
            // so we know which X32 main output corresponds to which channel / mix bus,
            // which is what we have meters for.
            for (int i = 1; i <= 16; i++)
            {
                ChannelId outputId = ChannelId.Output(mainOutputToChannelId[i]);
                levels[i - 1] = (outputId, ToMeterLevel(blob, i + 39));
            }
            Receiver.ReceiveMeterLevels(levels);
        }

        private static MeterLevel ToMeterLevel(byte[] blob, int index)
        {
            float raw = BinaryPrimitives.ReadSingleLittleEndian(blob.AsSpan().Slice(index * 4 + 4));
            var db = raw == 0 ? double.NegativeInfinity : 20 * Math.Log10(raw);
            return MeterLevel.FromDb(db);
        }

        protected override void PopulateReceiverMap(Dictionary<string, Action<OscMessage>> map)
        {
            base.PopulateReceiverMap(map);

            // 0 = Off
            // 1 = Main L
            // 2 = Main R
            // 3 = M/C (unused by us)
            // 4-19 = Bus 1-16 (so our output channel IDs 1-16)
            // 20+ = unused by us
            for (int i = 1; i <= 16; i++)
            {
                int output = i;
                map[GetOutputMappingAddress(i)] = message =>
                {
                    int value = (int) message[0];
                    mainOutputToChannelId[output] = value switch
                    {
                        1 => ChannelId.MainOutputLeft.Value,
                        2 => ChannelId.MainOutputRight.Value,
                        >= 4 and <= 19 => value - 3,
                        _ => 0,
                    };
                };
            }
        }

        // Addresses

        protected override IEnumerable<ChannelId> GetPotentialInputChannels() =>
            Enumerable.Range(1, 32).Select(ChannelId.Input);
        protected override IEnumerable<ChannelId> GetPotentialOutputChannels() =>
            Enumerable.Range(1, 16).Select(ChannelId.Output).Append(ChannelId.MainOutputLeft);

        protected override string GetInputPrefix(ChannelId inputId) =>
            $"/ch/{inputId.Value:00}";

        protected override string GetOutputPrefix(ChannelId outputId) =>
            outputId.IsMainOutput ? "/main/st" : $"/bus/{outputId.Value:00}";

        private static string GetOutputMappingAddress(int mainOutputId) =>
            $"/outputs/main/{mainOutputId:00}/src";
    }
}
