using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DigiMixer;

public sealed class Mixer : IDisposable, INotifyPropertyChanged
{
    private readonly Dictionary<ChannelId, InputChannel> leftOrMonoInputChannelMap;
    private readonly Dictionary<ChannelId, InputChannel> rightInputChannelMap;
    private readonly Dictionary<ChannelId, OutputChannel> leftOrMonoOutputChannelMap;
    private readonly Dictionary<ChannelId, OutputChannel> rightOutputChannelMap;
    private readonly Dictionary<ChannelId, ChannelBase> allChannelMap;
    private readonly Dictionary<(ChannelId, ChannelId), InputOutputMapping> mappings;

    public IReadOnlyList<InputChannel> InputChannels { get; }
    public IReadOnlyList<OutputChannel> OutputChannels { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public InputChannel GetInputChannel(ChannelId channelId) => leftOrMonoInputChannelMap[channelId];
    public OutputChannel GetOutputChannel(ChannelId channelId) => leftOrMonoOutputChannelMap[channelId];

    public bool Connected => ApiWrapper.Connected;

    /// <summary>
    /// The current underlying API. This can change due to reconnections, so should only be
    /// used transiently.
    /// </summary>
    internal ApiWrapper ApiWrapper { get; }

    public MixerChannelConfiguration ChannelConfiguration { get; }

    private bool disposed;
    private readonly Task keepAliveTask;
    private readonly Task connectionCheckTask;
    private readonly ConnectionTiming connectionTiming;
    private readonly ILogger logger;
    
    private Mixer(ILogger logger, Func<IMixerApi> apiFactory, IMixerApi initialApi, MixerChannelConfiguration config, ConnectionTiming connectionTiming)
    {
        this.logger = logger;
        this.connectionTiming = connectionTiming;
        ChannelConfiguration = config;
        ApiWrapper = new ApiWrapper(logger, CreateAndSubscribeToApi, initialApi, connectionTiming);
        initialApi.RegisterReceiver(new MixerReceiver(this));

        OutputChannels = ChannelConfiguration.GetPossiblyPairedOutputs()
            .Select(output => new OutputChannel(this, output))
            .ToList()
            .AsReadOnly();
        InputChannels = ChannelConfiguration.GetPossiblyPairedInputs()
            .Select(input => new InputChannel(this, input, OutputChannels))
            .ToList()
            .AsReadOnly();

        leftOrMonoInputChannelMap = InputChannels.ToDictionary(ch => ch.LeftOrMonoChannelId);
        leftOrMonoOutputChannelMap = OutputChannels.ToDictionary(ch => ch.LeftOrMonoChannelId);
        rightInputChannelMap = InputChannels.Where(ch => ch.IsStereo).ToDictionary(ch => ch.RightChannelId!.Value);
        rightOutputChannelMap = OutputChannels.Where(ch => ch.IsStereo).ToDictionary(ch => ch.RightChannelId!.Value);
        allChannelMap = leftOrMonoInputChannelMap.Select(pair => KeyValuePair.Create(pair.Key, (ChannelBase) pair.Value))
            .Concat(leftOrMonoOutputChannelMap.Select(pair => KeyValuePair.Create(pair.Key, (ChannelBase) pair.Value)))
            .Concat(rightInputChannelMap.Select(pair => KeyValuePair.Create(pair.Key, (ChannelBase) pair.Value)))
            .Concat(rightOutputChannelMap.Select(pair => KeyValuePair.Create(pair.Key, (ChannelBase) pair.Value)))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        // We assume that even for split faders, we're happy to only *report* via a single input/output channel pair.
        // (We will ignore any information about other combinations.)
        mappings = InputChannels.SelectMany(ic => ic.OutputMappings).ToDictionary(om => (om.InputChannel.LeftOrMonoChannelId, om.OutputChannel.LeftOrMonoChannelId));
        keepAliveTask = StartKeepAliveTask();
        connectionCheckTask = StartConnectionCheckTask();

        async Task<IMixerApi> CreateAndSubscribeToApi()
        {
            var api = apiFactory();
            using var cts = new CancellationTokenSource(connectionTiming.ConnectionTimeout);
            await api.Connect(cts.Token);
            api.RegisterReceiver(new MixerReceiver(this));
            await api.RequestAllData(ChannelConfiguration.InputChannels.Concat(ChannelConfiguration.OutputChannels).ToList().AsReadOnly());
            return api;
        }
    }

    private void RequestAllData() =>
        ApiWrapper.RequestAllData(ChannelConfiguration.InputChannels.Concat(ChannelConfiguration.OutputChannels).ToList().AsReadOnly());

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
            mixer.RequestAllData();
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

    private async Task StartKeepAliveTask()
    {
        while (!disposed)
        {
            ApiWrapper.SendKeepAlive();
            await Task.Delay(ApiWrapper.KeepAliveInterval);
        }
    }

    private async Task StartConnectionCheckTask()
    {
        while (!disposed)
        {
            // TODO: Propagate this from ApiWrapper instead.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Connected)));
            ApiWrapper.CheckConnection();
            await Task.Delay(connectionTiming.ConnectionCheckInterval);
        }
    }

    private MixerInfo? mixerInfo;
    public MixerInfo? MixerInfo
    {
        get => mixerInfo;
        set => this.SetProperty(PropertyChanged, ref mixerInfo, value);
    }

    // TODO: threading of receiving? (For meters in particular, best to invoke just once...)
    // For OSC, we get this for free due to awaiting...

    private bool TryGetOutputChannel(ChannelId channelId, [NotNullWhen(true)] out OutputChannel? channel) =>
        leftOrMonoOutputChannelMap.TryGetValue(channelId, out channel);

    private bool TryGetInputOutputMapping(ChannelId inputId, ChannelId outputId, [NotNullWhen(true)] out InputOutputMapping? mapping) =>
        mappings.TryGetValue((inputId, outputId), out mapping);

    private bool TryGetChannelBase(ChannelId channelId, [NotNullWhen(true)] out ChannelBase? channel) =>
        allChannelMap.TryGetValue(channelId, out channel);

    private class MixerReceiver : IMixerReceiver
    {
        private readonly Mixer mixer;

        internal MixerReceiver(Mixer mixer)
        {
            this.mixer = mixer;
        }

        public void ReceiveChannelName(ChannelId channelId, string? name)
        {
            if (mixer.TryGetChannelBase(channelId, out var channel))
            {
                if (channelId == channel.LeftOrMonoChannelId)
                {
                    channel.LeftOrMonoName = name;
                }
                else
                {
                    channel.RightName = name;
                }
            }
        }

        public void ReceiveFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
        {
            if (mixer.TryGetInputOutputMapping(inputId, outputId, out var mapping))
            {
                mapping.FaderLevel = level;
            }
        }

        public void ReceiveFaderLevel(ChannelId outputId, FaderLevel level)
        {
            if (mixer.TryGetOutputChannel(outputId, out var channel))
            {
                channel.FaderLevel = level;
            }
        }

        public void ReceiveMeterLevels((ChannelId channelId, MeterLevel level)[] levels)
        {
            foreach (var (channelId, level) in levels)
            {
                if (mixer.TryGetChannelBase(channelId, out var channel))
                {
                    if (channelId == channel.LeftOrMonoChannelId)
                    {
                        channel.MeterLevel = level;
                    }
                    else
                    {
                        channel.StereoMeterLevel = level;
                    }
                }
            }
        }

        public void ReceiveMixerInfo(MixerInfo info) => mixer.MixerInfo = info;

        public void ReceiveMuteStatus(ChannelId channelId, bool muted)
        {
            if (mixer.TryGetChannelBase(channelId, out var channel))
            {
                channel.Muted = muted;
            }
        }
    }

    public void Dispose()
    {
        disposed = true;
        ApiWrapper.Dispose();
    }
}
