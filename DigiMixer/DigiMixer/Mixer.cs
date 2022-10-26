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

    public IMixerApi Api { get; }

    public MixerChannelConfiguration ChannelConfiguration { get; }

    private bool disposed;
    private readonly Task keepAliveTask;

    // TODO: Reconnection
    /*
    public Mixer(IMixerApi api, IReadOnlyList<(ChannelId, ChannelId?)> inputIds, IReadOnlyList<(OutputChannelId, OutputChannelId?)> outputIds)
    {
        Api = api;
        api.RegisterReceiver(new MixerReceiver(this));
        var primaryOutputIds = outputIds.Select(pair => pair.Item1).ToList();
        InputChannels = inputIds.Select(pair => new InputChannel(this, pair.Item1, pair.Item2, primaryOutputIds)).ToList().AsReadOnly();
        OutputChannels = outputIds.Select(pair => new OutputChannel(this, pair.Item1, pair.Item2)).ToList().AsReadOnly();

        primaryInputChannels = InputChannels.ToDictionary(c => c.ChannelId);
        primaryOutputChannels = OutputChannels.ToDictionary(c => c.ChannelId);
        stereoInputChannels = InputChannels.Where(c => c.StereoChannelId is not null).ToDictionary(c => c.StereoChannelId!.Value);
        stereoOutputChannels = OutputChannels.Where(c => c.StereoChannelId is not null).ToDictionary(c => c.StereoChannelId!.Value);
        mappings = InputChannels.SelectMany(ic => ic.OutputMappings).ToDictionary(om => (om.InputChannelId, om.OutputChannelId));
        // TODO: Wait until we connect?
        keepAliveTask = StartKeepAliveTask();
    }*/

    private Mixer(IMixerApi api, MixerChannelConfiguration config)
    {
        Api = api;
        ChannelConfiguration = config;
        api.RegisterReceiver(new MixerReceiver(this));

        OutputChannels = ChannelConfiguration.PossiblyPairedOutputs
            .Select(output => new OutputChannel(this, output))
            .ToList()
            .AsReadOnly();
        InputChannels = ChannelConfiguration.PossiblyPairedInputs
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
    }

    private Task RequestAllData() =>
        Api.RequestAllData(ChannelConfiguration.InputChannels.Concat(ChannelConfiguration.OutputChannels).ToList().AsReadOnly());

    public static async Task<Mixer> Detect(IMixerApi api)
    {
        await api.Connect();
        var config = await api.DetectConfiguration();
        var mixer = new Mixer(api, config);
        await mixer.RequestAllData();
        return mixer;
    }

    private async Task StartKeepAliveTask()
    {
        // TODO: What happens if we dispose during a keep-alive?
        // Do we need a memory barrier to access disposed properly?
        while (!disposed)
        {
            await Api.SendKeepAlive();
            await Task.Delay(3000);
        }
    }

    public async Task Start()
    {
        await Api.Connect();
        await RequestAllData();
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

        public void ReceiveChannelName(ChannelId channelId, string name)
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

        public void ReceiveMeterLevel(ChannelId channelId, MeterLevel level)
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
        Api.Dispose();
    }
}
