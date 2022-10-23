using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DigiMixer;

public sealed class Mixer : INotifyPropertyChanged
{
    private readonly Dictionary<ChannelId, InputChannel> inputChannelMap;
    private readonly Dictionary<ChannelId, OutputChannel> outputChannelMap;
    private readonly Dictionary<ChannelId, ChannelBase> allChannelMap;
    private readonly Dictionary<(ChannelId, ChannelId), InputOutputMapping> mappings;

    public IReadOnlyList<InputChannel> InputChannels { get; }
    public IReadOnlyList<OutputChannel> OutputChannels { get; }
    public IReadOnlyList<MonoOrStereoPairChannel<InputChannel>> PossiblyPairedInputChannels { get; }
    public IReadOnlyList<MonoOrStereoPairChannel<OutputChannel>> PossiblyPairedOutputChannels { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public InputChannel GetInputChannel(ChannelId channelId) => inputChannelMap[channelId];
    public OutputChannel GetOutputChannel(ChannelId channelId) => outputChannelMap[channelId];

    public IMixerApi Api { get; }

    public MixerChannelConfiguration ChannelConfiguration { get; }

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
        InputChannels = config.InputChannels.Select(inputChannelId => new InputChannel(this, inputChannelId, config.OutputChannels)).ToList().AsReadOnly();
        OutputChannels = config.OutputChannels.Select(outputChannelId => new OutputChannel(this, outputChannelId)).ToList().AsReadOnly();
        inputChannelMap = InputChannels.ToDictionary(ch => ch.ChannelId);
        outputChannelMap = OutputChannels.ToDictionary(ch => ch.ChannelId);
        allChannelMap = InputChannels.Concat<ChannelBase>(OutputChannels).ToDictionary(ch => ch.ChannelId);
        mappings = InputChannels.SelectMany(ic => ic.OutputMappings).ToDictionary(om => (om.InputChannelId, om.OutputChannelId));
        keepAliveTask = StartKeepAliveTask();

        PossiblyPairedInputChannels = ChannelConfiguration.PossiblyPairedInputs
            .Select(input => MonoOrStereoPairChannel<InputChannel>.Map(input, inputChannelMap))
            .ToList()
            .AsReadOnly();
        PossiblyPairedOutputChannels = ChannelConfiguration.PossiblyPairedOutputs
            .Select(input => MonoOrStereoPairChannel<OutputChannel>.Map(input, outputChannelMap))
            .ToList()
            .AsReadOnly();
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
        while (true)
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
        outputChannelMap.TryGetValue(channelId, out channel);

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
                channel.Name = name;
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
                channel.MeterLevel = level;
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
}
