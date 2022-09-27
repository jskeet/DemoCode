// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

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

    private readonly IOscClient client;
    private readonly ConcurrentBag<IMixerReceiver> receivers = new ConcurrentBag<IMixerReceiver>();

    private OscMixerApi(IOscClient client)
    {
        this.client = client;
        client.PacketReceived += ReceivePacket;
        receiverActionsByAddress = BuildReceiverMap();
    }

    public static OscMixerApi ForUdp(string host, int port) =>
        new OscMixerApi(new UdpOscClient(host, port));

    public void RegisterReceiver(IMixerReceiver receiver) =>
        receivers.Add(receiver);

    public Task RequestFaderLevel(InputChannelId inputId, OutputChannelId outputId) =>
        client.SendAsync(new OscMessage(XAir.GetFaderAddress(inputId, outputId)));

    public Task RequestMuteStatus(InputChannelId inputId) =>
        client.SendAsync(new OscMessage(XAir.GetMuteAddress(inputId)));

    public Task RequestMuteStatus(OutputChannelId outputId) =>
        client.SendAsync(new OscMessage(XAir.GetMuteAddress(outputId)));

    public Task RequestName(InputChannelId inputId) =>
        client.SendAsync(new OscMessage(XAir.GetNameAddress(inputId)));

    public Task RequestName(OutputChannelId outputId) =>
        client.SendAsync(new OscMessage(XAir.GetNameAddress(outputId)));

    public Task RequestChannelUpdates() =>
        client.SendAsync(new OscMessage(XAir.XRemoteAddress));

    public async Task RequestMeterUpdates()
    {
        await client.SendAsync(new OscMessage("/batchsubscribe", XAir.InputChannelLevelsMeter, XAir.InputChannelLevelsMeter, 0, 0, 0 /* fast */));
        await client.SendAsync(new OscMessage("/batchsubscribe", XAir.OutputChannelLevelsMeter, XAir.OutputChannelLevelsMeter, 0, 0, 0 /* fast */));
    }

    public Task SetFaderLevel(InputChannelId inputId, OutputChannelId outputId, FaderLevel level) =>
        client.SendAsync(new OscMessage(XAir.GetFaderAddress(inputId, outputId), FromFaderLevel(level)));

    public Task SetMuted(InputChannelId inputId, bool muted) =>
        client.SendAsync(new OscMessage(XAir.GetMuteAddress(inputId), muted ? 1 : 0));

    public Task SetMuted(OutputChannelId outputId, bool muted) =>
        client.SendAsync(new OscMessage(XAir.GetMuteAddress(outputId), muted ? 1 : 0));

    private void ReceivePacket(object? sender, OscPacket e)
    {
        if (receivers.IsEmpty)
        {
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

        // TODO: Don't assume X-Air...
        var inputs = Enumerable.Range(1, 16).Select(id => new InputChannelId(id)).Append(XAir.AuxInput);
        var outputs = Enumerable.Range(1, 6).Select(id => new OutputChannelId(id)).Append(XAir.MainOutput);

        foreach (var input in inputs)
        {
            ret[XAir.GetNameAddress(input)] = (receiver, message) => receiver.ReceiveChannelName(input, (string) message[0]);
            ret[XAir.GetMuteAddress(input)] = (receiver, message) => receiver.ReceiveMuteStatus(input, (int) message[0] == 0);
        }

        foreach (var output in outputs)
        {
            ret[XAir.GetNameAddress(output)] = (receiver, message) => receiver.ReceiveChannelName(output, (string) message[0]);
            ret[XAir.GetMuteAddress(output)] = (receiver, message) => receiver.ReceiveMuteStatus(output, (int) message[0] == 0);
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
                InputChannelId inputId = new InputChannelId(i);
                receiver.ReceiveMeterLevel(inputId, ToMeterLevel(blob, i - 1));
            }
        };

        ret[XAir.OutputChannelLevelsMeter] = (receiver, message) =>
        {
            var blob = (byte[]) message[0];
            for (int i = 1; i <= 6; i++)
            {
                OutputChannelId outputId = new OutputChannelId(i);
                receiver.ReceiveMeterLevel(outputId, ToMeterLevel(blob, i - 1));
            }
            receiver.ReceiveMeterLevel(XAir.MainOutput, ToMeterLevel(blob, 6));
            receiver.ReceiveMeterLevel(XAir.MainOutputRightMeter, ToMeterLevel(blob, 7));
        };

        static MeterLevel ToMeterLevel(byte[] blob, int index)
        {
            short level = BitConverter.ToInt16(blob, index * 2 + 4);
            return new MeterLevel(level / 256.0);
        }
        return ret;
    }
}
