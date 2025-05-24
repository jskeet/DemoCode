using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.TfSeries;

public static class TfMixer
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 50368, MixerApiOptions? options = null) =>
        new TfMixerApi(logger, host, port, options);
}

internal class TfMixerApi : IMixerApi
{
    public TfMixerApi(ILogger logger, string host, int port, MixerApiOptions? options)
    {
    }

    public TimeSpan KeepAliveInterval => throw new NotImplementedException();

    public IFaderScale FaderScale => throw new NotImplementedException();

    public Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Connect(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void RegisterReceiver(IMixerReceiver receiver)
    {
        throw new NotImplementedException();
    }

    public Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        throw new NotImplementedException();
    }

    public Task SendKeepAlive()
    {
        throw new NotImplementedException();
    }

    public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        throw new NotImplementedException();
    }

    public Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        throw new NotImplementedException();
    }

    public Task SetMuted(ChannelId channelId, bool muted)
    {
        throw new NotImplementedException();
    }
}