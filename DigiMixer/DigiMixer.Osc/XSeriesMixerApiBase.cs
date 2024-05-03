// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using OscCore;
using System.Diagnostics;

namespace DigiMixer.Osc;

internal abstract class XSeriesMixerApiBase : OscMixerApiBase
{
    protected const string XRemoteAddress = "/xremote";
    protected const string XInfoAddress = "/xinfo";

    protected abstract string InputChannelLevelsMeter { get; }
    protected abstract string OutputChannelLevelsMeter { get; }

    internal XSeriesMixerApiBase(ILogger logger, string host, int port, MixerApiOptions? options) : base(logger, logger => new UdpOscClient(logger, host, port), options)
    {
    }

    public override sealed async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        var stopwatch = Stopwatch.StartNew();
        await Client.SendAsync(new OscMessage(XInfoAddress));
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
        await RequestAdditionalData();
        stopwatch.Stop();
        Logger.LogTrace("Requested all data in {ms}ms", stopwatch.ElapsedMilliseconds);

        // In reality we get through all of these in about 50ms, but let's allow for a glitchy connection.
        CancellationToken CreateCancellationToken() => new CancellationTokenSource(TimeSpan.FromMilliseconds(250)).Token;
    }

    protected virtual Task RequestAdditionalData() => Task.CompletedTask;

    public override TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(3);

    public override sealed async Task SendKeepAlive()
    {
        await Client.SendAsync(new OscMessage(XRemoteAddress));
        if (Options?.MeterOptions?.UpdateFrequency is var frequency && frequency != MeterUpdateFrequency.Off)
        {
            var timeFactor = frequency switch
            {
                MeterUpdateFrequency.Fast => 1, // 50ms between updates
                MeterUpdateFrequency.Medium => 4, // 200ms between updates
                MeterUpdateFrequency.Slow => 10, // 1s between updates
                _ => throw new ArgumentOutOfRangeException(nameof(MeterOptions.UpdateFrequency))
            };
            await Client.SendAsync(new OscMessage("/batchsubscribe", InputChannelLevelsMeter, InputChannelLevelsMeter, 0, 0, timeFactor));
            await Client.SendAsync(new OscMessage("/batchsubscribe", OutputChannelLevelsMeter, OutputChannelLevelsMeter, 0, 0, timeFactor));
        }
    }

    public override sealed async Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        var result = await InfoReceiver.RequestAndWait(Client, cancellationToken, XInfoAddress);
        return result is not null;
    }

    protected override void PopulateReceiverMap(Dictionary<string, Action<OscMessage>> map)
    {
        map[XInfoAddress] = message =>
        {
            string version = (string) message[3];
            string model = (string) message[2];
            string name = (string) message[1];
            Receiver.ReceiveMixerInfo(new MixerInfo(model, name, version));
        };

        map[InputChannelLevelsMeter] = ReceiveInputMeters;
        map[OutputChannelLevelsMeter] = ReceiveOutputMeters;
    }

    protected abstract void ReceiveInputMeters(OscMessage message);
    protected abstract void ReceiveOutputMeters(OscMessage message);

    // Addresses

    protected override sealed string GetFaderAddress(ChannelId inputId, ChannelId outputId)
    {
        string prefix = GetInputPrefix(inputId);
        return prefix + (outputId.IsMainOutput ? "/mix/fader" : $"/mix/{outputId.Value:00}/level");
    }

    protected override sealed object MutedValue { get; } = 0;
    protected override sealed object UnmutedValue { get; } = 1;
    protected override sealed string GetFaderAddress(ChannelId outputId) => GetOutputPrefix(outputId) + "/mix/fader";
    protected override sealed string GetMuteAddress(ChannelId channelId) => GetPrefix(channelId) + "/mix/on";
    protected override sealed string GetNameAddress(ChannelId channelId) => GetPrefix(channelId) + "/config/name";

    protected abstract string GetInputPrefix(ChannelId inputId);
    protected abstract string GetOutputPrefix(ChannelId outputId);

    private string GetPrefix(ChannelId channelId) =>
        channelId.IsInput ? GetInputPrefix(channelId) : GetOutputPrefix(channelId);
}
