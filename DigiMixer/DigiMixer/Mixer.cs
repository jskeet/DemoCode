using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DigiMixer;

public sealed class Mixer : INotifyPropertyChanged
{
    // TODO: Use arrays instead, with suitable guarantees in InputChannelId? Maybe change the underlying type to byte?
    private readonly Dictionary<InputChannelId, InputChannel> primaryInputChannels;
    private readonly Dictionary<OutputChannelId, OutputChannel> primaryOutputChannels;
    private readonly Dictionary<InputChannelId, InputChannel> stereoInputChannels;
    private readonly Dictionary<OutputChannelId, OutputChannel> stereoOutputChannels;
    private readonly Dictionary<(InputChannelId, OutputChannelId), InputOutputMapping> mappings;

    public IReadOnlyList<InputChannel> InputChannels { get; }
    public IReadOnlyList<OutputChannel> OutputChannels { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public IMixerApi Api { get; }

    private readonly Task keepAliveTask;

    // TODO: Reconnection

    public Mixer(IMixerApi api, IReadOnlyList<(InputChannelId, InputChannelId?)> inputIds, IReadOnlyList<(OutputChannelId, OutputChannelId?)> outputIds)
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
        await Api.RequestAllData(
            InputChannels.Select(c => c.ChannelId).ToList(),
            OutputChannels.Select(c => c.ChannelId).ToList());
    }

    private MixerInfo? mixerInfo;
    public MixerInfo? MixerInfo
    {
        get => mixerInfo;
        set => this.SetProperty(PropertyChanged, ref mixerInfo, value);
    }

    // TODO: threading of receiving? (For meters in particular, best to invoke just once...)
    // For OSC, we get this for free due to awaiting...

    private bool TryGetPrimaryInputChannel(InputChannelId channelId, [NotNullWhen(true)] out InputChannel? channel) =>
        primaryInputChannels.TryGetValue(channelId, out channel);

    private bool TryGetPrimaryOutputChannel(OutputChannelId channelId, [NotNullWhen(true)] out OutputChannel? channel) =>
        primaryOutputChannels.TryGetValue(channelId, out channel);

    private bool TryGetStereoInputChannel(InputChannelId channelId, [NotNullWhen(true)] out InputChannel? channel) =>
        stereoInputChannels.TryGetValue(channelId, out channel);

    private bool TryGetStereoOutputChannel(OutputChannelId channelId, [NotNullWhen(true)] out OutputChannel? channel) =>
        stereoOutputChannels.TryGetValue(channelId, out channel);

    private bool TryGetInputOutputMapping(InputChannelId inputId, OutputChannelId outputId, [NotNullWhen(true)] out InputOutputMapping? mapping) =>
        mappings.TryGetValue((inputId, outputId), out mapping);

    private class MixerReceiver : IMixerReceiver
    {
        private readonly Mixer mixer;

        internal MixerReceiver(Mixer mixer)
        {
            this.mixer = mixer;
        }

        public void ReceiveChannelName(InputChannelId channelId, string name)
        {
            if (mixer.TryGetPrimaryInputChannel(channelId, out var channel))
            {
                channel.Name = name;
            }
        }

        public void ReceiveChannelName(OutputChannelId channelId, string name)
        {
            if (mixer.TryGetPrimaryOutputChannel(channelId, out var channel))
            {
                channel.Name = name;
            }
        }

        public void ReceiveFaderLevel(InputChannelId inputId, OutputChannelId outputId, FaderLevel level)
        {
            if (mixer.TryGetInputOutputMapping(inputId, outputId, out var mapping))
            {
                mapping.FaderLevel = level;
            }
        }

        public void ReceiveFaderLevel(OutputChannelId outputId, FaderLevel level)
        {
            if (mixer.TryGetPrimaryOutputChannel(outputId, out var channel))
            {
                channel.FaderLevel = level;
            }
        }

        public void ReceiveMeterLevel(InputChannelId channelId, MeterLevel level)
        {
            if (mixer.TryGetPrimaryInputChannel(channelId, out var channel))
            {
                channel.MeterLevel = level;
            }
            else if (mixer.TryGetStereoInputChannel(channelId, out channel))
            {
                channel.StereoMeterLevel = level;
            }
        }

        public void ReceiveMeterLevel(OutputChannelId channelId, MeterLevel level)
        {
            if (mixer.TryGetPrimaryOutputChannel(channelId, out var channel))
            {
                channel.MeterLevel = level;
            }
            else if (mixer.TryGetStereoOutputChannel(channelId, out channel))
            {
                channel.StereoMeterLevel = level;
            }
        }

        public void ReceiveMixerInfo(MixerInfo info) => mixer.MixerInfo = info;

        public void ReceiveMuteStatus(InputChannelId channelId, bool muted)
        {
            if (mixer.TryGetPrimaryInputChannel(channelId, out var channel))
            {
                channel.Muted = muted;
            }
        }

        public void ReceiveMuteStatus(OutputChannelId channelId, bool muted)
        {
            if (mixer.TryGetPrimaryOutputChannel(channelId, out var channel))
            {
                channel.Muted = muted;
            }
        }
    }
}
