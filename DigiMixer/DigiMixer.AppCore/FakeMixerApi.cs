using DigiMixer.Core;

namespace DigiMixer.AppCore;

/// <summary>
/// A fake <see cref="IMixerApi"/> implementation which can be used for testing,
/// or for offline use within a larger application (such as At Your Service).
/// </summary>
internal sealed class FakeMixerApi : IMixerApi
{
    private readonly DigiMixerConfig config;
    private readonly DelegatingReceiver receiver;

    internal FakeMixerApi(DigiMixerConfig config)
    {
        this.config = config;
        receiver = new DelegatingReceiver();
    }

    public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(1);
    public IFaderScale FaderScale { get; } = new LinearFaderScale((1, -100.0), (1000, 10.0));

    public Task<bool> CheckConnection(CancellationToken cancellationToken) => Task.FromResult(true);

    public Task Connect(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        var channelConfig = new MixerChannelConfiguration(
            config.InputChannels.Select(mapping => ChannelId.Input(mapping.Channel)),
            config.OutputChannels.Select(mapping => ChannelId.Output(mapping.Channel)),
            Enumerable.Empty<StereoPair>());
        return Task.FromResult(channelConfig);
    }

    public void Dispose()
    {
    }

    public void RegisterReceiver(IMixerReceiver receiver) => this.receiver.RegisterReceiver(receiver);

    public Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        // TODO: Call Receive methods for channels.
        return Task.CompletedTask;
    }

    public Task SendKeepAlive() => Task.CompletedTask;

    public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        receiver.ReceiveFaderLevel(inputId, outputId, level);
        return Task.CompletedTask;
    }

    public Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        receiver.ReceiveFaderLevel(outputId, level);
        return Task.CompletedTask;
    }

    public Task SetMuted(ChannelId channelId, bool muted)
    {
        receiver.ReceiveMuteStatus(channelId, muted);
        return Task.CompletedTask;
    }
}
