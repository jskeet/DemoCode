using DigiMixer.Core;

namespace DigiMixer;

public partial class Mixer
{
    private class NoOpApi : IMixerApi
    {
        internal static IMixerApi Instance { get; } = new NoOpApi();

        private NoOpApi() { }

        public Task Connect(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public void Dispose()
        {
        }

        public void RegisterReceiver(IMixerReceiver receiver)
        {
        }

        public Task RequestAllData(IReadOnlyList<ChannelId> channelIds) => Task.CompletedTask;

        public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level) => Task.CompletedTask;

        public Task SetFaderLevel(ChannelId outputId, FaderLevel level) => Task.CompletedTask;

        public Task SetMuted(ChannelId channelId, bool muted) => Task.CompletedTask;

        // Note that this should be reasonably short so that the Mixer sends a keepalive
        // soon after reconnecting.
        public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(1);

        public Task SendKeepAlive() => Task.CompletedTask;

        public Task<bool> CheckConnection(CancellationToken cancellationToken) => Task.FromResult(true);
    }
}
