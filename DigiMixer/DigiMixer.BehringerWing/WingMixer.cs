using DigiMixer.BehringerWing.Core;
using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace DigiMixer.BehringerWing;

/// <summary>
/// Mixer for the Behringer Wing series: Wing, Wing Compact, Wing Rack.
/// </summary>
public class WingMixer
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 2222, MixerApiOptions? options = null) =>
        new AutoReceiveMixerApi(new WingMixerApi(logger, host, port, options));
}

internal class WingMixerApi : IMixerApi
{
    private static DbFaderScale DbFaderScale { get; } = new(-89.5, -40, -30, -20, -10, -5, -1, 5, 10);

    private readonly DelegatingReceiver receiver = new();
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;
    private readonly MixerApiOptions options;
    private readonly ImmutableDictionary<uint, Action<WingToken>> valueActionsByHash;

    private CancellationTokenSource? cts;
    private WingClient? controlClient;

    public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(4);
    public IFaderScale FaderScale => DbFaderScale;
    private Queue<TaskCompletionSource> dataRequestCompletions = new();

    // This is only used during DetectConfiguration, but it's simpler to accumulate
    // state in general than to add actions that are only used temporarily.
    // This doesn't include output channels: all output channels are assumed to be stereo.
    private Dictionary<ChannelId, bool> monoOrStereoChannels = new();

    private uint currentHash = 0;

    internal WingMixerApi(ILogger logger, string host, int port, MixerApiOptions? options)
    {
        this.logger = logger;
        this.host = host;
        this.port = port;
        this.options = options ?? MixerApiOptions.Default;
        valueActionsByHash = CreateValueActionsDictionary();
    }

    private ImmutableDictionary<uint, Action<WingToken>> CreateValueActionsDictionary()
    {
        var builder = ImmutableDictionary.CreateBuilder<uint, Action<WingToken>>();
        Register(Hashes.AllChannelHashes, hash => hash.Name, (id, token) => receiver.ReceiveChannelName(id, EmptyToNull(token.StringValue)));
        Register(Hashes.AllChannelHashes, hash => hash.Mute, (id, token) => receiver.ReceiveMuteStatus(id, token.BoolValue));
        // We treat mid/side as stereo.
        Register(InputChannelHashes.AllInputs, hash => hash.StereoMode, (id, token) => monoOrStereoChannels[id] = token.StringValue == "MONO");
        Register(InputChannelHashes.AllInputs, hash => hash.Fader, (id, token) => receiver.ReceiveFaderLevel(id, ChannelId.MainOutputLeft, DbFaderScale.ConvertToFaderLevel(token.Float32Value)));
        Register(OutputChannelHashes.AllOutputs, hash => hash.Fader, (id, token) => receiver.ReceiveFaderLevel(id, DbFaderScale.ConvertToFaderLevel(token.Float32Value)));

        foreach (var inputHash in InputChannelHashes.AllInputs)
        {
            for (int outputIndex = 0; outputIndex < inputHash.OutputLevels.Count; outputIndex++)
            {
                var outputId = ChannelId.Output(outputIndex + 1);
                builder[inputHash.OutputLevels[outputIndex]] = token => receiver.ReceiveFaderLevel(inputHash.Id, outputId, DbFaderScale.ConvertToFaderLevel(token.Float32Value));
            }
        }

        return builder.ToImmutable();

        void Register<T>(IEnumerable<T> hashesCollection, Func<T, uint> hashSelector, Action<ChannelId, WingToken> action)
            where T : IChannelHashes
        {
            foreach (var hashes in hashesCollection)
            {
                var hash = hashSelector(hashes);
                builder[hash] = token => action(hashes.Id, token);
            }
        }

        string? EmptyToNull(string text) => text == "" ? null : text;
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        Dispose();

        cts = new CancellationTokenSource();
        controlClient = new WingClient(logger, host, port);
        controlClient.AudioEngineTokenReceived += HandleAudioEngineToken;
        await controlClient.Connect(cancellationToken);
        controlClient.Start();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
        await controlClient.SendAudioEngineTokens([WingToken.RootNode], linkedCts.Token);
    }

    private void HandleAudioEngineToken(object? sender, WingToken token)
    {
        if (token.Type is WingTokenType.NodeHash)
        {
            currentHash = token.NodeHash;
        }
        else if (token.Type is WingTokenType.EndOfRequest)
        {
            // Let anything waiting for their request to complete know that it's done.
            // We assume the mixer handles multiple requests in order.
            if (dataRequestCompletions.TryDequeue(out var tcs))
            {
                tcs.TrySetResult();
            }
        }
        else if (token.Type is WingTokenType.Int16 or WingTokenType.Int32 or WingTokenType.Float32 or WingTokenType.String or WingTokenType.FalseOffZero or WingTokenType.TrueOnOne)
        {
            if (valueActionsByHash.TryGetValue(currentHash, out var action))
            {
                action(token);
            }
            currentHash = 0;
        }
    }

    public void RegisterReceiver(IMixerReceiver receiver) => this.receiver.RegisterReceiver(receiver);

    public async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        // This will populate monoOrStereoChannels.
        await RequestAllData(cancellationToken);

        var stereoLeftInputChannels = monoOrStereoChannels.Where(pair => pair.Value).Select(pair => pair.Key);

        var inputChannels = Hashes.AllInputChannels.Concat(stereoLeftInputChannels.Select(RightStereoChannel));
        var outputChannels = Hashes.AllOutputChannels
            .Concat(Hashes.AllOutputChannels.Select(RightStereoChannel))
            // We need this as an output so we can use the "Main LR" faders. But we don't actually want to see a fader/meter for it. Bit weird.
            .Append(ChannelId.MainOutputLeft);
        var stereoPairs = stereoLeftInputChannels.Concat(Hashes.AllOutputChannels)
            .Select(left => new StereoPair(left, RightStereoChannel(left), StereoFlags.None));
        return new(inputChannels, outputChannels, stereoPairs);
    }

    // This requests more than we really need - really *everything* - but it's fine.
    public Task RequestAllData(IReadOnlyList<ChannelId> channelIds) => RequestAllData(cts?.Token ?? default);

    private async Task RequestAllData(CancellationToken cancellationToken)
    {
        if (controlClient is null)
        {
            throw new InvalidOperationException("Client is not connected");
        }
        var completeTcs = new TaskCompletionSource();
        dataRequestCompletions.Enqueue(completeTcs);
        await SendTokens([WingToken.RootNode, WingToken.DataRequest], cancellationToken);
        using (cancellationToken.Register(() => completeTcs.TrySetCanceled()))
        {
            await completeTcs.Task;
        }
        // Fake the name of the "main" pseudo-channel.
        receiver.ReceiveChannelName(ChannelId.MainOutputLeft, "(MainLR)");
    }

    public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        var hashes = InputChannelHashes.AllInputsByChannelId[inputId];
        var hash = outputId.IsMainOutput ? hashes.Fader : hashes.OutputLevels[outputId.Value - 1];
        var value = WingToken.ForFloat32((float) DbFaderScale.ConvertToDb(level.Value));
        return SetNodeValue(hash, value);
    }

    public Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        var hashes = OutputChannelHashes.AllOutputsByChannelId[outputId];
        var value = WingToken.ForFloat32((float) DbFaderScale.ConvertToDb(level.Value));
        return SetNodeValue(hashes.Fader, value);
    }

    public Task SetMuted(ChannelId channelId, bool muted)
    {
        // We don't really have a "main output" to mute.
        if (channelId.IsMainOutput)
        {
            return Task.CompletedTask;
        }
        var hashes = Hashes.AllChannelHashesByChannelId[channelId];
        return SetNodeValue(hashes.Mute, WingToken.ForBool(muted));
    }

    public async Task SendKeepAlive()
    {
        if (controlClient is not null && cts is not null)
        {
            // TODO: Send a meter request instead, when we've implemented metering.
            // Hopefully sending this token won't be harmful, but there's no genuine "no-op" as far as I can tell.
            await controlClient.SendAudioEngineTokens([WingToken.EndOfRequest], cts.Token);
        }
    }

    private Task SetNodeValue(uint nodeHash, WingToken value) =>
        SendTokens([WingToken.ForNodeHash(nodeHash), value], null);

    private async Task SendTokens(IEnumerable<WingToken> tokens, CancellationToken? cancellationToken)
    {
        CancellationToken ct = cancellationToken ?? cts?.Token ?? default;
        if (controlClient is not null)
        {
            await controlClient.SendAudioEngineTokens(tokens, ct);
        }
    }

    public Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        // Propagate any existing client failures.
        if (controlClient?.ControllerStatus != ControllerStatus.Running)
        {
            logger.LogInformation("Returning unhealthy as control or meter client has failed");
            return Task.FromResult(false);
        }

        // TODO: Actually use the connection (which could be tricky if we're requesting data).
        // Possibly use "have we seen meter data"
        return Task.FromResult(true);
    }

    public void Dispose()
    {
        controlClient?.Dispose();
        controlClient = null;
    }

    // We model the "right" channels as the "left" channels + 1000, with a hope that nothing assumes
    // they're contiguous.
    private static ChannelId RightStereoChannel(ChannelId left) => left.WithValue(left.Value + 1000);
}
