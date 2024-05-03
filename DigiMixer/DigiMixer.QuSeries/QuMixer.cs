using DigiMixer.Core;
using DigiMixer.QuSeries.Core;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace DigiMixer.QuSeries;

public class QuMixer
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 51326, MixerApiOptions? options = null) =>
        new AutoReceiveMixerApi(new QuMixerApi(logger, host, port, options));
}

internal class QuMixerApi : IMixerApi
{
    private readonly DelegatingReceiver receiver = new();
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;
    private readonly LinkedList<MessageListener> temporaryListeners = new();
    private readonly LinkedList<Action<QuControlMessage>> messageHandlers = new();

    private CancellationTokenSource? cts;
    private QuControlClient? controlClient;
    private QuMeterClient? meterClient;
    private IPEndPoint? mixerUdpEndPoint;
    private readonly MixerApiOptions options;

    private MixerInfo currentMixerInfo = MixerInfo.Empty;

    internal QuMixerApi(ILogger logger, string host, int port, MixerApiOptions? options)
    {
        this.logger = logger;
        this.host = host;
        this.port = port;
        this.options = options ?? MixerApiOptions.Default;
        AddMessageHandlers();
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        Dispose();

        cts = new CancellationTokenSource();
        meterClient = new QuMeterClient(logger);
        meterClient.MessageReceived += HandleMeterMessage;
        meterClient.Start();
        controlClient = new QuControlClient(logger, host, port);
        controlClient.MessageReceived += HandleControlMessage;
        await controlClient.Connect(cancellationToken);
        controlClient.Start();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
        await controlClient.SendAsync(QuMessages.InitialHandshakeRequest(meterClient.LocalUdpPort), linkedCts.Token);
        await controlClient.SendAsync(QuMessages.RequestControlMessages, linkedCts.Token);
        // TODO: Wait until we've had a reply?
    }

    public async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        var message = await RequestData(QuMessages.RequestFullData, QuMessages.FullDataType, cancellationToken);
        var data = new FullDataMessage(message);

        return data.CreateChannelConfiguration();
    }

    public void RegisterReceiver(IMixerReceiver receiver) => this.receiver.RegisterReceiver(receiver);

    public async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        await SendMessage(QuMessages.RequestNetworkInformation);
        await SendMessage(QuMessages.RequestVersionInformation);
        await SendMessage(QuMessages.RequestFullData);
    }

    public async Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        // Propagate any existing client failures.
        if (controlClient?.ControllerStatus != ControllerStatus.Running ||
            meterClient?.ControllerStatus != ControllerStatus.Running)
        {
            return false;
        }
        await RequestData(QuMessages.RequestVersionInformation, QuMessages.VersionInformationType, cancellationToken);
        return true;
    }

    // TODO: Check this.
    public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(3);
    public IFaderScale FaderScale => QuConversions.FaderScale;

    public async Task SendKeepAlive()
    {
        if (meterClient is not null && cts is not null && mixerUdpEndPoint is not null)
        {
            await meterClient.SendKeepAliveAsync(mixerUdpEndPoint, cts.Token);
        }
    }

    public async Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        int address =
            outputId.IsMainOutput ? 0x07_00_04_07
            // Mono mix
            : outputId.Value < 5 ? (outputId.Value - 1) << 24
            // Stereo mix
            : outputId.Value < 20 ? ((outputId.Value - 5) / 2 + 4) << 24
            // Groups
            : ((outputId.Value - 20) / 2 + 8) << 24;
        if (!outputId.IsMainOutput)
        {
            address |= 0x0c_0a;
        }
        address |= (inputId.Value - 1) << 16;
        var message = new QuValueMessage(4, 4, address, QuConversions.FaderLevelToRaw(level));
        await SendMessage(message);
    }

    public async Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        int networkChannelId = QuConversions.ChannelIdToNetwork(outputId);
        int address = 0x07_00_04_07 | (networkChannelId << 16);
        var message = new QuValueMessage(4, 4, address, QuConversions.FaderLevelToRaw(level));
        await SendMessage(message);
    }

    public async Task SetMuted(ChannelId channelId, bool muted)
    {
        int networkChannelId = QuConversions.ChannelIdToNetwork(channelId);
        int address = 0x07_00_00_06 | (networkChannelId << 16);
        var message = new QuValueMessage(4, 4, address, (ushort) (muted ? 1 : 0));
        await SendMessage(message);
    }

    private async Task<QuGeneralMessage> RequestData(QuControlMessage requestMessage, byte expectedResponseType, CancellationToken cancellationToken)
    {
        if (controlClient is null || cts is null)
        {
            throw new InvalidOperationException("Not connected");
        }
        // TODO: thread safety...
        var listener = new MessageListener(message => message is QuGeneralMessage qgm && qgm.Type == expectedResponseType, cancellationToken);
        temporaryListeners.AddLast(listener);
        await controlClient.SendAsync(requestMessage, cts.Token);
        return (QuGeneralMessage) await listener.Task;
    }

    private async Task SendMessage(QuControlMessage message)
    {
        if (controlClient is not null)
        {
            await controlClient.SendAsync(message, cts?.Token ?? default);
        }
    }

    private void HandleControlMessage(object? sender, QuControlMessage message)
    {
        if (QuMessages.IsInitialHandshakeResponse(message, out var mixerUdpPort))
        {
            this.mixerUdpEndPoint = new IPEndPoint(IPAddress.Parse(host), mixerUdpPort);
            return;
        }
        var node = temporaryListeners.First;
        while (node is not null)
        {
            if (node.Value.HandleMessage(message))
            {
                node.Value.Dispose();
                temporaryListeners.Remove(node);
            }
            node = node.Next;
        }
        foreach (var handler in messageHandlers)
        {
            handler(message);
        }
    }

    private void HandleMeterMessage(object? sender, QuGeneralMessage message)
    {
        if (message.Type == QuMessages.InputMeterType)
        {
            var data = message.Data;
            // TODO: Don't hard code this. (Where should we remember it?)
            var meters = new (ChannelId, MeterLevel)[32];
            for (int i = 0; i < meters.Length; i++)
            {
                var slice = data.Slice(i * 20, 20);
                meters[i] = (ChannelId.Input(i + 1), QuConversions.RawToMeterLevel(MemoryMarshal.Read<ushort>(slice.Slice(6, 2))));
            }
            receiver.ReceiveMeterLevels(meters);
        }
        else if (message.Type == QuMessages.OutputMeterType)
        {
            var data = message.Data;
            // TODO: Don't hard code this. (Where should we remember it?)
            int index = 0;
            var meters = new (ChannelId, MeterLevel)[20];
            // Mono and stereo mixes (each 20 bytes, starting at 0)
            for (int i = 0; i < 10; i++)
            {
                var slice = data.Slice(i * 20, 20);
                meters[index++] = (ChannelId.Output(i + 1), QuConversions.RawToMeterLevel(MemoryMarshal.Read<ushort>(slice.Slice(10, 2))));
            }
            // Groups (each 20 bytes, starting at 240)
            // These are actually left/right pairs, but we can handle them like this.
            for (int i = 0; i < 8; i++)
            {
                var slice = data.Slice(i * 20 + 240, 40);
                meters[index++] = (QuConversions.GroupChannelId(i + 1), QuConversions.RawToMeterLevel(MemoryMarshal.Read<ushort>(slice.Slice(10, 2))));
            }
            meters[index++] = (ChannelId.MainOutputLeft, QuConversions.RawToMeterLevel(MemoryMarshal.Read<ushort>(data.Slice(200, 20).Slice(10, 2))));
            meters[index++] = (ChannelId.MainOutputRight, QuConversions.RawToMeterLevel(MemoryMarshal.Read<ushort>(data.Slice(220, 20).Slice(10, 2))));
            receiver.ReceiveMeterLevels(meters);
        }
    }

    private void HandleFullDataMessage(QuGeneralMessage message)
    {
        var data = new FullDataMessage(message);
        // Note that for stereo channels, we'll end up reporting values twice. That's okay.
        for (int channel = 1; channel <= data.InputCount; channel++)
        {
            var channelId = ChannelId.Input(channel);
            receiver.ReceiveMuteStatus(channelId, data.InputMuted(channel));
            receiver.ReceiveChannelName(channelId, data.GetInputName(channel));
            receiver.ReceiveFaderLevel(channelId, ChannelId.MainOutputLeft, data.InputFaderLevel(channel));

            for (int mix = 1; mix <= data.MonoMixChannels + data.StereoMixChannels; mix++)
            {
                var mixId = ChannelId.Output(mix);
                receiver.ReceiveFaderLevel(channelId, mixId, data.InputMixFaderLevel(channel, mix));
            }

            for (int group = 1; group <= data.StereoGroupChannels; group += 2)
            {
                var groupId = QuConversions.GroupChannelId(group);
                receiver.ReceiveFaderLevel(channelId, groupId, data.InputGroupFaderLevel(channel, group));
            }
        }

        for (int mix = 1; mix <= data.MonoMixChannels + data.StereoMixChannels; mix++)
        {
            var mixId = ChannelId.Output(mix);
            receiver.ReceiveMuteStatus(mixId, data.MixMuted(mix));
            receiver.ReceiveChannelName(mixId, data.GetMixName(mix));
            receiver.ReceiveFaderLevel(mixId, data.MixFaderLevel(mix));
        }

        for (int group = 1; group <= data.StereoGroupChannels; group++)
        {
            var groupId = QuConversions.GroupChannelId(group);
            receiver.ReceiveMuteStatus(groupId, data.GroupMuted(group));
            receiver.ReceiveChannelName(groupId, data.GetGroupName(group));
            receiver.ReceiveFaderLevel(groupId, data.GroupFaderLevel(group));
        }

        receiver.ReceiveMuteStatus(ChannelId.MainOutputLeft, data.MainMuted());
        receiver.ReceiveChannelName(ChannelId.MainOutputLeft, data.GetMainName() ?? "Main");
        receiver.ReceiveFaderLevel(ChannelId.MainOutputLeft, data.MainFaderLevel());
    }

    private void HandleNetworkInformationMessage(QuGeneralMessage message)
    {
        // Network information message:
        // IP address (4 bytes)
        // Subnet mask (4 bytes)
        // Gateway (4 bytes)
        // DHCP enabled (1 byte)
        // Name (15 bytes)
        // ???? (4 bytes)
        var data = message.Data;
        string name = Encoding.ASCII.GetString(data.Slice(13, 15));
        currentMixerInfo = currentMixerInfo with { Name = name };
        receiver.ReceiveMixerInfo(currentMixerInfo);
    }

    private void HandleVersionInformationMessage(QuGeneralMessage message)
    {
        // Note: the model is probably encoded in the first two bytes, but that's hard to check...
        var data = message.Data;
        int firmwareMajor = data[3];
        int firmwareMinor = data[2];
        ushort revision = MemoryMarshal.Read<ushort>(data.Slice(4, 2));
        currentMixerInfo = currentMixerInfo with { Model = "Qu-???", Version = $"{firmwareMajor}.{firmwareMinor} rev {revision}" };
        receiver.ReceiveMixerInfo(currentMixerInfo);
    }

    private void HandleValueMessage(QuValueMessage message)
    {
        // logger.LogInformation("Received value message: Client: {client}; Section: {section}; Address: {address}; Value:{value}", message.ClientId, message.Section, $"0x{message.Address:x8}", message.RawValue);

        // Fader and mute
        if (message.Section == 4 && (message.Address & 0xff_00_00_00) == 0x07_00_00_00)
        {
            int networkChannel = (message.Address & 0x00_ff_00_00) >> 16;
            ChannelId? possibleChannelId = QuConversions.NetworkToChannelId(networkChannel);
            if (possibleChannelId is not ChannelId channelId)
            {
                return;
            }
            if ((message.Address & 0xff) == 0x07)
            {
                if (channelId.IsInput)
                {
                    receiver.ReceiveFaderLevel(channelId, ChannelId.MainOutputLeft, QuConversions.RawToFaderLevel(message.RawValue));
                }
                else
                {
                    receiver.ReceiveFaderLevel(channelId, QuConversions.RawToFaderLevel(message.RawValue));
                }
            }
            else if ((message.Address & 0xff) == 0x06)
            {
                receiver.ReceiveMuteStatus(channelId, message.RawValue == 1);
            }
        }
        else if (message.Section == 4 && (message.Address & 0xff_ff) == 0x0c_0a)
        {
            int output = (message.Address >> 24) + 1;
            ChannelId outputId =
                output < 5 ? ChannelId.Output(output)
                : output < 9 ? ChannelId.Output((output - 5) * 2 + 5)
                : output < 16 ? QuConversions.GroupChannelId((output - 9) * 2 + 1)
                : ChannelId.Output(output); // TODO: FX Send
            int input = ((message.Address & 0xff_00_00) >> 16) + 1;
            receiver.ReceiveFaderLevel(ChannelId.Input(input), outputId, QuConversions.RawToFaderLevel(message.RawValue));
        }
    }

    private void AddMessageHandlers()
    {
        AddGeneralHandlerForType(QuMessages.FullDataType, HandleFullDataMessage);
        AddGeneralHandlerForType(QuMessages.NetworkInformationType, HandleNetworkInformationMessage);
        AddGeneralHandlerForType(QuMessages.VersionInformationType, HandleVersionInformationMessage);
        messageHandlers.AddLast(message =>
        {
            if (message is QuValueMessage qvm)
            {
                HandleValueMessage(qvm);
            }
        });

        void AddGeneralHandlerForType(byte type, Action<QuGeneralMessage> action)
        {
            messageHandlers.AddLast(message =>
            {
                if (message is QuGeneralMessage general && general.Type == type)
                {
                    action(general);
                }
            });
        }
    }

    public void Dispose()
    {
        controlClient?.Dispose();
        controlClient = null;
        meterClient?.Dispose();
        meterClient = null;
        mixerUdpEndPoint = null;
    }

    private class MessageListener : IDisposable
    {
        private readonly TaskCompletionSource<QuControlMessage> tcs;
        private readonly Func<QuControlMessage, bool> predicate;
        private readonly CancellationTokenRegistration ctr;

        internal Task<QuControlMessage> Task => tcs.Task;

        internal MessageListener(Func<QuControlMessage, bool> predicate, CancellationToken cancellationToken)
        {
            tcs = new TaskCompletionSource<QuControlMessage>();
            ctr = cancellationToken.Register(() => tcs.TrySetCanceled());
            this.predicate = predicate;
        }

        internal bool HandleMessage(QuControlMessage message)
        {
            // If we've already cancelled the task, the listener is done.
            if (tcs.Task.IsCanceled)
            {
                return true;
            }
            if (!predicate(message))
            {
                return false;
            }
            tcs.TrySetResult(message);
            return true;
        }

        public void Dispose()
        {
            ctr.Unregister();
        }
    }
}
