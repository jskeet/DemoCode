namespace DigiMixer.Core;

/// <summary>
/// Implementation of <see cref="IMixerApi"/> which delegates all
/// calls to another implementation, but calls <see cref="IMixerReceiver"/>
/// methods when any "set" methods are called. This can be used to
/// wrap implementations which don't receive "echo" messages from
/// the hardware as confirmation of changes.
/// </summary>
public class AutoReceiveMixerApi : IMixerApi
{
    private readonly IMixerApi target;
    private readonly DelegatingReceiver receiver = new();

    public AutoReceiveMixerApi(IMixerApi target)
    {
        this.target = target;
    }

    public Task Connect() => target.Connect();

    public Task<MixerChannelConfiguration> DetectConfiguration() =>
        target.DetectConfiguration();

    public void Dispose() => target.Dispose();

    public void RegisterReceiver(IMixerReceiver receiver)
    {
        this.receiver.RegisterReceiver(receiver);
        target.RegisterReceiver(receiver);
    }

    public Task RequestAllData(IReadOnlyList<ChannelId> channelIds) =>
        target.RequestAllData(channelIds);

    public Task SendKeepAlive() => target.SendKeepAlive();

    public async Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        await target.SetFaderLevel(inputId, outputId, level);
        receiver.ReceiveFaderLevel(inputId, outputId, level);
    }

    public async Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        await target.SetFaderLevel(outputId, level);
        receiver.ReceiveFaderLevel(outputId, level);
    }

    public async Task SetMuted(ChannelId channelId, bool muted)
    {
        await target.SetMuted(channelId, muted);
        receiver.ReceiveMuteStatus(channelId, muted);
    }
}
