// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OscCore;
using OscMixerControl;
using System.Collections.Concurrent;

namespace DigiMixer.Osc;

/// <summary>
/// Implementation of <see cref="IMixerApi"/> for Open Sound Control.
/// </summary>
/// <remarks>
/// TODO: Extract out the address mapping to avoid the hard XAir dependency.
/// </remarks>
public class OscMixerApi : IMixerApi
{
    // Note: not static as it could be different for different mixers, even just XR12 vs XR16 vs XR18 vs X32
    private readonly Dictionary<string, Action<IMixerReceiver, OscMessage>> receiverActionsByAddress;

    private readonly ILogger logger;
    private readonly Func<IOscClient> clientProvider;
    private IOscClient client;
    private Task receivingTask;
    private readonly ConcurrentBag<IMixerReceiver> receivers = new ConcurrentBag<IMixerReceiver>();

    private OscMixerApi(ILogger logger, Func<IOscClient> clientProvider)
    {
        this.clientProvider = clientProvider;
        this.logger = logger;
        client = IOscClient.Fake.Instance;
        receivingTask = Task.CompletedTask;
        receiverActionsByAddress = BuildReceiverMap();
    }

    public Task Connect()
    {
        client.Dispose();
        var newClient = clientProvider();
        newClient.PacketReceived += ReceivePacket;
        receivingTask = newClient.StartReceiving();
        client = newClient;
        return Task.CompletedTask;
    }
    
    public async Task<MixerChannelConfiguration> DetectConfiguration()
    {
        var token = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
        var result = await InfoReceiver.RequestAndWait(client, token,
            XAir.InfoAddress,
            XAir.InputChannelLinkAddress,
            XAir.BusChannelLinkAddress);
        var inputLinks = result[XAir.InputChannelLinkAddress].Select(x => x is 1).ToList();
        var outputLinks = result[XAir.BusChannelLinkAddress].Select(x => x is 1).ToList();
        var model = (string) result[XAir.InfoAddress][2];

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

    public static OscMixerApi ForUdp(ILogger? logger, string host, int port)
    {
        logger = logger ?? NullLogger.Instance;

        return new OscMixerApi(logger, () => new UdpOscClient(logger, host, port));
    }

    public void RegisterReceiver(IMixerReceiver receiver) =>
        receivers.Add(receiver);

    public async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        await client.SendAsync(new OscMessage(XAir.InfoAddress));
        // TODO: Apply some rigour to the delays - potentially wait for responses using InfoReceiver?
        foreach (var channelId in channelIds)
        {
            await client.SendAsync(new OscMessage(XAir.GetMuteAddress(channelId)));
            await client.SendAsync(new OscMessage(XAir.GetNameAddress(channelId)));
            if (channelId.IsOutput)
            {
                await client.SendAsync(new OscMessage(XAir.GetFaderAddress(channelId)));
            }
            await Task.Delay(20);
        }

        foreach (var input in channelIds.Where(c => c.IsInput))
        {
            foreach (var output in channelIds.Where(c => c.IsOutput))
            {
                await client.SendAsync(new OscMessage(XAir.GetFaderAddress(input, output)));
            }
            await Task.Delay(20);
        }
    }

    public async Task SendKeepAlive()
    {
        await client.SendAsync(new OscMessage(XAir.XRemoteAddress));
        await client.SendAsync(new OscMessage("/batchsubscribe", XAir.InputChannelLevelsMeter, XAir.InputChannelLevelsMeter, 0, 0, 0 /* fast */));
        await client.SendAsync(new OscMessage("/batchsubscribe", XAir.OutputChannelLevelsMeter, XAir.OutputChannelLevelsMeter, 0, 0, 0 /* fast */));
    }

    public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level) =>
        client.SendAsync(new OscMessage(XAir.GetFaderAddress(inputId, outputId), FromFaderLevel(level)));

    public Task SetFaderLevel(ChannelId outputId, FaderLevel level) =>
        client.SendAsync(new OscMessage(XAir.GetFaderAddress(outputId), FromFaderLevel(level)));

    public Task SetMuted(ChannelId channelId, bool muted) =>
        client.SendAsync(new OscMessage(XAir.GetMuteAddress(channelId), muted ? 0 : 1));

    private void ReceivePacket(object? sender, OscPacket e)
    {
        if (receivers.IsEmpty)
        {
            return;
        }
        if (e is OscBundle bundle)
        {
            foreach (var bundleMessage in (IEnumerable<OscMessage>) bundle)
            {
                ReceivePacket(sender, bundleMessage);
            }
            return;
        }
        if (e is not OscMessage message)
        {
            return;
        }
        var address = message.Address;
        if (!receiverActionsByAddress.TryGetValue(address, out var action))
        {
            return;
        }
        foreach (var receiver in receivers)
        {
            action(receiver, message);
        }
    }

    private static float FromFaderLevel(FaderLevel level) => level.Value / (float) FaderLevel.MaxValue;

    private static FaderLevel ToFaderLevel(float value) => new FaderLevel((int) (value * FaderLevel.MaxValue));

    private static Dictionary<string, Action<IMixerReceiver, OscMessage>> BuildReceiverMap()
    {
        var ret = new Dictionary<string, Action<IMixerReceiver, OscMessage>>();

        ret[XAir.InfoAddress] = (receiver, message) =>
        {
            string version = (string) message[0] + " / " + (string) message[3];
            string model = (string) message[2];
            string name = (string) message[1];
            receiver.ReceiveMixerInfo(new MixerInfo(model, name, version));
        };

        // TODO: Don't assume X-Air...
        // TODO: Populate this on-demand? Would avoid assumptions about the number of inputs/outputs...
        var inputs = Enumerable.Range(1, 16).Select(id => new ChannelId(id, input: true)).Append(XAir.AuxInputLeft);
        var outputs = Enumerable.Range(1, 6).Select(id => new ChannelId(id, input: false)).Append(XAir.MainOutputLeft);

        foreach (var input in inputs)
        {
            ret[XAir.GetNameAddress(input)] = (receiver, message) => receiver.ReceiveChannelName(input, (string) message[0]);
            ret[XAir.GetMuteAddress(input)] = (receiver, message) => receiver.ReceiveMuteStatus(input, (int) message[0] == 0);
        }

        foreach (var output in outputs)
        {
            ret[XAir.GetNameAddress(output)] = (receiver, message) => receiver.ReceiveChannelName(output, (string) message[0]);
            ret[XAir.GetMuteAddress(output)] = (receiver, message) => receiver.ReceiveMuteStatus(output, (int) message[0] == 0);
            ret[XAir.GetFaderAddress(output)] = (receiver, message) => receiver.ReceiveFaderLevel(output, ToFaderLevel((float) message[0]));
        }

        foreach (var input in inputs)
        {
            foreach (var output in outputs)
            {
                ret[XAir.GetFaderAddress(input, output)] = (receiver, message) => receiver.ReceiveFaderLevel(input, output, ToFaderLevel((float) message[0]));
            }
        }

        ret[XAir.InputChannelLevelsMeter] = (receiver, message) =>
        {
            var blob = (byte[]) message[0];
            for (int i = 1; i <= 18; i++)
            {
                ChannelId inputId = new ChannelId(i, input: true);
                receiver.ReceiveMeterLevel(inputId, ToMeterLevel(blob, i - 1));
            }
        };

        ret[XAir.OutputChannelLevelsMeter] = (receiver, message) =>
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
            return new MeterLevel(level / 256.0);
        }
        return ret;
    }

    public void Dispose() => client.Dispose();
}
