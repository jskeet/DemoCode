// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OscCore;

namespace DigiMixer.Osc;

/// <summary>
/// Base implementation of <see cref="IMixerApi"/> for Open Sound Control.
/// Different mixers use different OSC addresses, and are represented via subclasses.
/// </summary>
internal abstract class OscMixerApiBase : IMixerApi
{
    // Note: not static as it could be different for different mixers, even just XR12 vs XR16 vs XR18 vs X32
    private readonly Dictionary<string, Action<OscMessage>> receiverActionsByAddress;

    protected ILogger Logger { get; }
    private readonly Func<ILogger, IOscClient> clientProvider;
    protected IOscClient Client { get; private set; }
    private readonly DelegatingReceiver receiver = new();

    protected IMixerReceiver Receiver => receiver;

    protected abstract object MutedValue { get; }
    protected abstract object UnmutedValue { get; }

    private protected OscMixerApiBase(ILogger logger, Func<ILogger, IOscClient> clientProvider)
    {
        this.clientProvider = clientProvider;
        Logger = logger ?? NullLogger.Instance;
        Client = IOscClient.Fake.Instance;
        receiverActionsByAddress = BuildReceiverMap();
    }

    public virtual async Task Connect(CancellationToken cancellationToken)
    {
        Client.Dispose();
        var newClient = clientProvider(Logger);
        newClient.PacketReceived += ReceivePacket;
        newClient.Start();
        Client = newClient;
        // Only return when we're definitely connected.
        if (!await CheckConnection(cancellationToken))
        {
            throw new InvalidOperationException("Unable to connect successfully.");
        }
    }

    public abstract Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken);
    public abstract Task SendKeepAlive();
    public abstract Task<bool> CheckConnection(CancellationToken cancellationToken);
    public abstract TimeSpan KeepAliveInterval { get; }
    protected abstract string GetFaderAddress(ChannelId inputId, ChannelId outputId);
    protected abstract string GetFaderAddress(ChannelId outputId);
    protected abstract string GetMuteAddress(ChannelId channelId);
    protected abstract string GetNameAddress(ChannelId channelId);
    /// <summary>
    /// Populates the address to receiver action map for any addresses which aren't just faders/mutes/names.
    /// </summary>
    protected abstract void PopulateReceiverMap(Dictionary<string, Action<OscMessage>> map);

    // TODO: Document this more
    protected abstract IEnumerable<ChannelId> GetPotentialInputChannels();
    protected abstract IEnumerable<ChannelId> GetPotentialOutputChannels();

    public void RegisterReceiver(IMixerReceiver receiver) =>
        this.receiver.RegisterReceiver(receiver);

    public abstract Task RequestAllData(IReadOnlyList<ChannelId> channelIds);

    public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level) =>
        Client.SendAsync(new OscMessage(GetFaderAddress(inputId, outputId), FromFaderLevel(level)));

    public Task SetFaderLevel(ChannelId outputId, FaderLevel level) =>
        Client.SendAsync(new OscMessage(GetFaderAddress(outputId), FromFaderLevel(level)));

    public Task SetMuted(ChannelId channelId, bool muted) =>
        Client.SendAsync(new OscMessage(GetMuteAddress(channelId), muted ? MutedValue : UnmutedValue));

    private void ReceivePacket(object? sender, OscPacket e)
    {
        if (receiver.IsEmpty)
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
            // Logger.LogTrace("Unknown OSC address '{address}'", address);
            return;
        }
        action(message);
    }

    private static float FromFaderLevel(FaderLevel level) => level.Value / (float) FaderLevel.MaxValue;

    private static FaderLevel ToFaderLevel(float value) => new FaderLevel((int) (value * FaderLevel.MaxValue));

    private Dictionary<string, Action<OscMessage>> BuildReceiverMap()
    {
        var ret = new Dictionary<string, Action<OscMessage>>();

        // TODO: Populate this on-demand? Would avoid assumptions about the number of inputs/outputs...
        var inputs = GetPotentialInputChannels();
        var outputs = GetPotentialOutputChannels();

        foreach (var input in inputs)
        {
            ret[GetNameAddress(input)] = message => receiver.ReceiveChannelName(input, (string) message[0]);
            ret[GetMuteAddress(input)] = message => receiver.ReceiveMuteStatus(input, Equals(MutedValue, message[0]));
        }

        foreach (var output in outputs)
        {
            ret[GetNameAddress(output)] = message => receiver.ReceiveChannelName(output, (string) message[0]);
            ret[GetMuteAddress(output)] = message => receiver.ReceiveMuteStatus(output, Equals(MutedValue, message[0]));
            ret[GetFaderAddress(output)] = message => receiver.ReceiveFaderLevel(output, ToFaderLevel((float) message[0]));
        }

        foreach (var input in inputs)
        {
            foreach (var output in outputs)
            {
                ret[GetFaderAddress(input, output)] = message => receiver.ReceiveFaderLevel(input, output, ToFaderLevel((float) message[0]));
            }
        }

        PopulateReceiverMap(ret);
        return ret;
    }

    public void Dispose() => Client.Dispose();
}
