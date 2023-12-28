// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using OscCore;
using System.Buffers.Binary;

namespace DigiMixer.Osc;

/// <summary>
/// Factory methods and constants for working with X-Air mixers
/// (e.g. XR12, XR16, XR18).
/// </summary>
public static class XAir
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 10024) =>
        new XAirOscMixerApi(logger, host, port);

    private class XAirOscMixerApi : XSeriesMixerApiBase
    {
        /// <summary>
        /// The input channel ID for the left "aux" input.
        /// </summary>
        private static ChannelId AuxInputLeft { get; } = ChannelId.Input(17);

        /// <summary>
        /// The input channel ID for the right "aux" input.
        /// </summary>
        private static ChannelId AuxInputRight { get; } = ChannelId.Input(18);

        private const string InputChannelLinkAddress = "/config/chlink";
        private const string BusChannelLinkAddress = "/config/buslink";

        internal XAirOscMixerApi(ILogger logger, string host, int port) : base(logger, host, port)
        {
        }

        protected override string InputChannelLevelsMeter { get; } = "/meters/1";
        protected override string OutputChannelLevelsMeter { get; } = "/meters/5";

        public override async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
        {
            var result = await InfoReceiver.RequestAndWait(Client, cancellationToken,
                XInfoAddress,
                InputChannelLinkAddress,
                BusChannelLinkAddress);
            if (result is null)
            {
                throw new InvalidOperationException("Detection timed out");
            }
            var inputLinks = result[InputChannelLinkAddress].Select(x => x is 1).ToList();
            var outputLinks = result[BusChannelLinkAddress].Select(x => x is 1).ToList();
            var model = (string) result[XInfoAddress][2];

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
                .Append(ChannelId.MainOutputLeft).Append(ChannelId.MainOutputRight);

            var stereoPairs = CreateStereoPairs(inputCount, inputLinks, ChannelId.Input)
                .Concat(CreateStereoPairs(outputCount, outputLinks, ChannelId.Output))
                .Append(new StereoPair(ChannelId.MainOutputLeft, ChannelId.MainOutputRight, StereoFlags.None));
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

        protected override void ReceiveInputMeters(OscMessage message)
        {
            var levels = new (ChannelId, MeterLevel)[18];
            var blob = (byte[]) message[0];
            for (int i = 1; i <= 18; i++)
            {
                ChannelId inputId = ChannelId.Input(i);
                levels[i - 1] = (inputId, ToMeterLevel(blob, i - 1));
            }
            Receiver.ReceiveMeterLevels(levels);
        }

        protected override void ReceiveOutputMeters(OscMessage message)
        {
            var levels = new (ChannelId, MeterLevel)[8];
            var blob = (byte[]) message[0];
            for (int i = 1; i <= 6; i++)
            {
                ChannelId outputId = ChannelId.Output(i);
                levels[i - 1] = (outputId, ToMeterLevel(blob, i - 1));
            }
            levels[6] = (ChannelId.MainOutputLeft, ToMeterLevel(blob, 6));
            levels[7] = (ChannelId.MainOutputRight, ToMeterLevel(blob, 7));
            Receiver.ReceiveMeterLevels(levels);
        }

        private static MeterLevel ToMeterLevel(byte[] blob, int index)
        {
            short level = BinaryPrimitives.ReadInt16LittleEndian(blob.AsSpan().Slice(index * 2 + 4));
            return MeterLevel.FromDb(level / 256.0);
        }

        // Addresses

        protected override IEnumerable<ChannelId> GetPotentialInputChannels() =>
            Enumerable.Range(1, 16).Select(ChannelId.Input).Append(AuxInputLeft);
        protected override IEnumerable<ChannelId> GetPotentialOutputChannels() =>
            Enumerable.Range(1, 6).Select(ChannelId.Output).Append(ChannelId.MainOutputLeft);

        protected override string GetInputPrefix(ChannelId inputId) =>
            IsAuxInput(inputId) ? "/rtn/aux" : $"/ch/{inputId.Value:00}";

        protected override string GetOutputPrefix(ChannelId outputId) =>
            outputId.IsMainOutput ? "/lr" : $"/bus/{outputId.Value}";

        private static bool IsAuxInput(ChannelId channelId) =>
            channelId == AuxInputLeft || channelId == AuxInputRight;
    }
}
