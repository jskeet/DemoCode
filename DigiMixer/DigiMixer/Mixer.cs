using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DigiMixer;

public sealed partial class Mixer : IDisposable, INotifyPropertyChanged
{
    private readonly Func<Task<IMixerApi>> apiFactory;
    private readonly ConnectionTiming connectionTiming;
    private readonly ILogger logger;

    public MixerChannelConfiguration ChannelConfiguration { get; }
    public IReadOnlyList<InputChannel> InputChannels { get; }
    public IReadOnlyList<OutputChannel> OutputChannels { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    // Mutable state...

    /// <summary>
    /// The current underlying API.
    /// This can change due to reconnections, so should only be used transiently.
    /// </summary>
    private IMixerApi api;
    private bool disposed;

    private bool connected;
    public bool Connected
    {
        get => connected;
        set => this.SetProperty(PropertyChanged, ref connected, value);
    }

    private MixerInfo? mixerInfo;
    public MixerInfo? MixerInfo
    {
        get => mixerInfo;
        private set => this.SetProperty(PropertyChanged, ref mixerInfo, value);
    }

    private Mixer(ILogger logger, Func<IMixerApi> apiFactory, IMixerApi initialApi, MixerChannelConfiguration config, ConnectionTiming connectionTiming)
    {
        this.logger = logger;
        this.connectionTiming = connectionTiming;
        api = initialApi;

        ChannelConfiguration = config;
        Connected = true;
        OutputChannels = ChannelConfiguration.GetPossiblyPairedOutputs()
            .Select(output => new OutputChannel(this, output))
            .ToList()
            .AsReadOnly();
        InputChannels = ChannelConfiguration.GetPossiblyPairedInputs()
            .Select(input => new InputChannel(this, input, OutputChannels))
            .ToList()
            .AsReadOnly();

        var receiver = new MixerReceiver(this);
        initialApi.RegisterReceiver(receiver);

        this.apiFactory = CreateAndSubscribeToApi;
        LogErrors(StartKeepAliveTask());
        LogErrors(StartConnectionCheckTask());

        async Task<IMixerApi> CreateAndSubscribeToApi()
        {
            var api = apiFactory();
            bool dispose = true;
            try
            {
                using var cts = new CancellationTokenSource(connectionTiming.ConnectionTimeout);
                await api.Connect(cts.Token);
                api.RegisterReceiver(receiver);
                // TODO: Should this have a cancellation token, as it's part of connection?
                await api.RequestAllData(ChannelConfiguration.InputChannels.Concat(ChannelConfiguration.OutputChannels).ToList().AsReadOnly());
                dispose = false;
            }
            finally
            {
                if (dispose)
                {
                    api.Dispose();
                }
            }
            return api;
        }
    }

    public static async Task<Mixer> Create(ILogger logger, Func<IMixerApi> apiFactory, ConnectionTiming? timing = null)
    {
        timing ??= new ConnectionTiming();
        var api = apiFactory();
        bool success = false;
        try
        {
            using var cts = new CancellationTokenSource(timing.ConnectionTimeout);
            await api.Connect(cts.Token);
            var config = await api.DetectConfiguration(cts.Token);
            var mixer = new Mixer(logger, apiFactory, api, config, timing);
            mixer.RequestAllData(config.InputChannels.Concat(config.OutputChannels).ToList().AsReadOnly());
            success = true;
            return mixer;
        }
        finally
        {
            if (!success)
            {
                api.Dispose();
            }
        }
    }

    internal void SetFaderLevel(ChannelId outputId, FaderLevel level) =>
        LogErrors(api.SetFaderLevel(outputId, level));

    internal void SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level) =>
        LogErrors(api.SetFaderLevel(inputId, outputId, level));

    internal void SetMuted(ChannelId channelId, bool muted) =>
        LogErrors(api.SetMuted(channelId, muted));

    internal void RequestAllData(ReadOnlyCollection<ChannelId> channels) =>
        LogErrors(api.RequestAllData(channels));

    private async Task StartKeepAliveTask()
    {
        while (!disposed)
        {
            LogErrors(api.SendKeepAlive());
            await Task.Delay(api.KeepAliveInterval);
        }
    }

    private async Task StartConnectionCheckTask()
    {
        while (!disposed)
        {
            await CheckConnectionAndMaybeReconnect();
            await Task.Delay(connectionTiming.ConnectionCheckInterval);
        }

        async Task CheckConnectionAndMaybeReconnect()
        {
            try
            {
                logger.LogTrace("Checking connection.");
                using var cts = new CancellationTokenSource(connectionTiming.ConnectionCheckTimeout);
                var result = await api.CheckConnection(cts.Token);

                // Normal result: everything is fine.
                if (result)
                {
                    return;
                }
                logger.LogError("Mixer API reported unhealthy connection; reconnecting.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error checking mixer connection status; assuming unhealthy and reconnecting.");
            }

            Connected = false;

            // Dispose of the current API and stop further calls to it by using a placeholder while we're reconnecting.
            api.Dispose();
            api = NoOpApi.Instance;

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
                    api = candidateApi;
                    Connected = true;
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

                if (reconnectCounter == connectionTiming.InitialReconnectionAttempts)
                {
                    logger.LogInformation("Initial reconnection attempts failed; count={count}. Quietly trying to reconnect less frequently.", connectionTiming.InitialReconnectionAttempts);
                }
            }
        }
    }

    private void LogErrors(Task task)
    {
        task.ContinueWith(t => logger.LogError(t.Exception, "Mixer error"), TaskContinuationOptions.NotOnRanToCompletion);
    }

    public void Dispose()
    {
        disposed = true;
        api.Dispose();
    }
}
