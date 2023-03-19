using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace DigiMixer;

// TODO: Consider putting all of this back into Mixer. Does it actually need to be separate?
// Try in a separate commit, that's easy to remove.
// (In particular CheckConnection does sync to async, whereas it's already async in Mixer...)

/// <summary>
/// A wrapper around an <see cref="IMixerApi"/>, making it simpler to use from channels
/// and from <see cref="Mixer"/>.
/// </summary>
internal class ApiWrapper : IDisposable
{
    private readonly Func<Task<IMixerApi>> apiFactory;
    private readonly ConnectionTiming connectionTiming;
    private readonly ILogger logger;
    private IMixerApi currentApi;
    private bool disposed = false;
    private Task? reconnectionTask;
    private Task<bool>? connectionCheckTask;

    /// <summary>
    /// Note
    /// </summary>
    /// <param name="apiFactory">
    /// A factory to asynchronously create new mixer APIs after failure.
    /// The mixer creating the wrapper is expected to provide a factory that
    /// connects and already has an appropriate subscription set up.
    /// </param>
    /// <param name="initialApi">The initial, already-connected mixer API</param>
    /// <param name="connectionTiming"></param>
    internal ApiWrapper(ILogger logger, Func<Task<IMixerApi>> apiFactory, IMixerApi initialApi, ConnectionTiming connectionTiming)
    {
        this.apiFactory = apiFactory;
        this.currentApi = initialApi;
        this.connectionTiming = connectionTiming;
        this.logger = logger;
    }

    internal bool Connected => reconnectionTask is null;

    internal TimeSpan KeepAliveInterval => currentApi.KeepAliveInterval;

    internal void CheckConnection()
    {
        // If we're already in the middle of reconnecting, just let that continue.
        if (reconnectionTask is not null || connectionCheckTask is not null)
        {
            return;
        }

        try
        {
            connectionCheckTask = CheckConnection();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error checking mixer connection status; assuming unhealthy and reconnecting.");
            connectionCheckTask = null;
            reconnectionTask = StartReconnecting();
            return;
        }

        connectionCheckTask.ContinueWith(task =>
        {
            // Whether successful or not, we can create new connection checks now.
            connectionCheckTask = null;
            if (task.IsFaulted || task.IsCanceled || (task.IsCompleted && !task.Result))
            {
                logger.LogError("Mixer API reported unhealthy connection; reconnecting.");
                reconnectionTask = StartReconnecting();
            }
        });
        return;

        async Task<bool> CheckConnection()
        {
            logger.LogInformation("Checking connection.");
            using var cts = new CancellationTokenSource(connectionTiming.ConnectionCheckTimeout);
            return await currentApi.CheckConnection(cts.Token);
        }

        // Initial reconnection
        async Task StartReconnecting()
        {
            // Allow the calling code to assign a value to reconnectionTask, in case we're able
            // to reconnect synchronously.
            //await Task.Yield();

            // Dispose of the current API and stop further calls to it by using a placeholder while we're reconnecting.
            logger.LogInformation("Disposing of API.");
            currentApi.Dispose();
            logger.LogInformation("Disposed.");
            currentApi = NoOpApi.Instance;

            long reconnectCounter = 0;
            while (!disposed)
            {
                if (reconnectCounter < connectionTiming.InitialReconnectionAttempts)
                {
                    logger.LogInformation("Attempting reconnection");
                }

                try
                {
                    var candidateApi = await apiFactory();
                    logger.LogInformation("Reconnection successful.");
                    currentApi = candidateApi;
                    reconnectionTask = null;
                    return;
                }
                catch
                {
                    // Ignore the error, wait and try again.
                }

                var interval = reconnectCounter < connectionTiming.InitialReconnectionAttempts
                    ? connectionTiming.InitialReconnectionInterval : connectionTiming.EventualReconnectionInterval;
                await Task.Delay(interval);
                reconnectCounter++;
            }
        }
    }

    internal void SendKeepAlive()
    {
        LogErrors(currentApi.SendKeepAlive());
    }

    public void Dispose()
    {
        disposed = true;
        currentApi.Dispose();
        currentApi = NoOpApi.Instance;
    }

    public void SetFaderLevel(ChannelId outputId, FaderLevel level) =>
        LogErrors(currentApi.SetFaderLevel(outputId, level));

    public void SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level) =>
        LogErrors(currentApi.SetFaderLevel(inputId, outputId, level));

    public void SetMuted(ChannelId channelId, bool muted) =>
        LogErrors(currentApi.SetMuted(channelId, muted));

    public void RequestAllData(ReadOnlyCollection<ChannelId> channels) =>
        LogErrors(currentApi.RequestAllData(channels));

    public void LogErrors(Task task)
    {
        task.ContinueWith(t => logger.LogError(t.Exception, "Mixer error"), TaskContinuationOptions.OnlyOnFaulted);
    }

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
