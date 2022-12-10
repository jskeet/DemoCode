﻿// Copyright 2022 Jon Skeet. All rights reserved.
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
/// Base implementation of <see cref="IMixerApi"/> for Open Sound Control.
/// Different mixers use different OSC addresses, and are represented via subclasses.
/// </summary>
internal abstract class OscMixerApiBase : IMixerApi
{
    // Note: not static as it could be different for different mixers, even just XR12 vs XR16 vs XR18 vs X32
    private readonly Dictionary<string, Action<IMixerReceiver, OscMessage>> receiverActionsByAddress;

    protected ILogger Logger { get; }
    private readonly Func<ILogger, IOscClient> clientProvider;
    protected IOscClient Client { get; private set; }
    private Task receivingTask;
    private readonly ConcurrentBag<IMixerReceiver> receivers = new ConcurrentBag<IMixerReceiver>();

    protected abstract object MutedValue { get; }
    protected abstract object UnmutedValue { get; }

    private protected OscMixerApiBase(ILogger logger, Func<ILogger, IOscClient> clientProvider)
    {
        this.clientProvider = clientProvider;
        Logger = logger ?? NullLogger.Instance;
        Client = IOscClient.Fake.Instance;
        receivingTask = Task.CompletedTask;
        receiverActionsByAddress = BuildReceiverMap();
    }

    public virtual Task Connect()
    {
        Client.Dispose();
        var newClient = clientProvider(Logger);
        newClient.PacketReceived += ReceivePacket;
        receivingTask = newClient.StartReceiving();
        Client = newClient;
        return Task.CompletedTask;
    }

    public abstract Task<MixerChannelConfiguration> DetectConfiguration();
    public abstract Task SendKeepAlive();
    protected abstract string GetFaderAddress(ChannelId inputId, ChannelId outputId);
    protected abstract string GetFaderAddress(ChannelId outputId);
    protected abstract string GetMuteAddress(ChannelId channelId);
    protected abstract string GetNameAddress(ChannelId channelId);
    /// <summary>
    /// Populates the address to receiver action map for any addresses which aren't just faders/mutes/names.
    /// </summary>
    protected abstract void PopulateReceiverMap(Dictionary<string, Action<IMixerReceiver, OscMessage>> map);

    // TODO: Document this more
    protected abstract IEnumerable<ChannelId> GetPotentialInputChannels();
    protected abstract IEnumerable<ChannelId> GetPotentialOutputChannels();

    public void RegisterReceiver(IMixerReceiver receiver) =>
        receivers.Add(receiver);

    public abstract Task RequestAllData(IReadOnlyList<ChannelId> channelIds);

    public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level) =>
        Client.SendAsync(new OscMessage(GetFaderAddress(inputId, outputId), FromFaderLevel(level)));

    public Task SetFaderLevel(ChannelId outputId, FaderLevel level) =>
        Client.SendAsync(new OscMessage(GetFaderAddress(outputId), FromFaderLevel(level)));

    public Task SetMuted(ChannelId channelId, bool muted) =>
        Client.SendAsync(new OscMessage(GetMuteAddress(channelId), muted ? MutedValue : UnmutedValue));

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

    private Dictionary<string, Action<IMixerReceiver, OscMessage>> BuildReceiverMap()
    {
        var ret = new Dictionary<string, Action<IMixerReceiver, OscMessage>>();

        // TODO: Populate this on-demand? Would avoid assumptions about the number of inputs/outputs...
        var inputs = GetPotentialInputChannels();
        var outputs = GetPotentialOutputChannels();

        foreach (var input in inputs)
        {
            ret[GetNameAddress(input)] = (receiver, message) => receiver.ReceiveChannelName(input, (string) message[0]);
            ret[GetMuteAddress(input)] = (receiver, message) => receiver.ReceiveMuteStatus(input, Equals(MutedValue, message[0]));
        }

        foreach (var output in outputs)
        {
            ret[GetNameAddress(output)] = (receiver, message) => receiver.ReceiveChannelName(output, (string) message[0]);
            ret[GetMuteAddress(output)] = (receiver, message) => receiver.ReceiveMuteStatus(output, Equals(MutedValue, message[0]));
            ret[GetFaderAddress(output)] = (receiver, message) => receiver.ReceiveFaderLevel(output, ToFaderLevel((float) message[0]));
        }

        foreach (var input in inputs)
        {
            foreach (var output in outputs)
            {
                ret[GetFaderAddress(input, output)] = (receiver, message) => receiver.ReceiveFaderLevel(input, output, ToFaderLevel((float) message[0]));
            }
        }

        PopulateReceiverMap(ret);
        return ret;
    }

    protected void Broadcast(Action<IMixerReceiver> action)
    {
        foreach (var receiver in receivers)
        {
            action(receiver);
        }
    }

    public void Dispose() => Client.Dispose();
}
