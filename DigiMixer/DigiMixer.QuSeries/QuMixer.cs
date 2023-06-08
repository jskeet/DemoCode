using DigiMixer.Core;
using DigiMixer.QuSeries.Core;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace DigiMixer.QuSeries;

public class QuMixer
{
    public static IMixerApi CreateMixerApi(ILogger logger, string host, int port = 51326) =>
        new AutoReceiveMixerApi(new QuMixerApi(logger, host, port));
}

internal class QuMixerApi : IMixerApi
{
    private readonly DelegatingReceiver receiver = new();
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;
    private readonly LinkedList<PacketListener> temporaryListeners = new();
    private readonly LinkedList<Action<QuControlPacket>> packetHandlers = new();

    private CancellationTokenSource? cts;
    private QuControlClient? controlClient;
    private QuMeterClient? meterClient;
    private IPEndPoint? mixerUdpEndPoint;

    private MixerInfo currentMixerInfo = MixerInfo.Empty;

    internal QuMixerApi(ILogger logger, string host, int port)
    {
        this.logger = logger;
        this.host = host;
        this.port = port;
        AddPacketHandlers();
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        Dispose();

        cts = new CancellationTokenSource();
        meterClient = new QuMeterClient(logger);
        meterClient.PacketReceived += HandleMeterPacket;
        meterClient.Start();
        controlClient = new QuControlClient(logger, host, port);
        controlClient.PacketReceived += HandleControlPacket;
        await controlClient.Connect(cancellationToken);
        controlClient.Start();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
        await controlClient.SendAsync(QuPackets.InitialHandshakeRequest(meterClient.LocalUdpPort), linkedCts.Token);
        await controlClient.SendAsync(QuPackets.RequestControlPackets, linkedCts.Token);
        // TODO: Wait until we've had a reply?
    }

    public async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        var packet = await RequestData(QuPackets.RequestFullData, QuPackets.FullDataType, cancellationToken);
        var data = new FullDataPacket(packet);

        return data.CreateChannelConfiguration();
    }

    public void RegisterReceiver(IMixerReceiver receiver) => this.receiver.RegisterReceiver(receiver);

    public async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        await SendPacket(QuPackets.RequestNetworkInformation);
        await SendPacket(QuPackets.RequestVersionInformation);
        await SendPacket(QuPackets.RequestFullData);
    }

    public async Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        // Propagate any existing client failures.
        if (controlClient?.ControllerStatus != ControllerStatus.Running ||
            meterClient?.ControllerStatus != ControllerStatus.Running)
        {
            return false;
        }
        await RequestData(QuPackets.RequestVersionInformation, QuPackets.VersionInformationType, cancellationToken);
        return true;
    }

    // TODO: Check this.
    public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(3);

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
        var packet = new QuValuePacket(4, 4, address, QuConversions.FaderLevelToRaw(level));
        await SendPacket(packet);
    }

    public async Task SetFaderLevel(ChannelId outputId, FaderLevel level)
    {
        int networkChannelId = QuConversions.ChannelIdToNetwork(outputId);
        int address = 0x07_00_04_07 | (networkChannelId << 16);
        var packet = new QuValuePacket(4, 4, address, QuConversions.FaderLevelToRaw(level));
        await SendPacket(packet);
    }

    public async Task SetMuted(ChannelId channelId, bool muted)
    {
        int networkChannelId = QuConversions.ChannelIdToNetwork(channelId);
        int address = 0x07_00_00_06 | (networkChannelId << 16);
        var packet = new QuValuePacket(4, 4, address, (ushort) (muted ? 1 : 0));
        await SendPacket(packet);
    }

    private async Task<QuGeneralPacket> RequestData(QuControlPacket requestPacket, byte expectedResponseType, CancellationToken cancellationToken)
    {
        if (controlClient is null || cts is null)
        {
            throw new InvalidOperationException("Not connected");
        }
        // TODO: thread safety...
        var listener = new PacketListener(packet => packet is QuGeneralPacket qgp && qgp.Type == expectedResponseType, cancellationToken);
        temporaryListeners.AddLast(listener);
        await controlClient.SendAsync(requestPacket, cts.Token);
        return (QuGeneralPacket) await listener.Task;
    }

    private async Task SendPacket(QuControlPacket packet)
    {
        if (controlClient is not null)
        {
            await controlClient.SendAsync(packet, cts?.Token ?? default);
        }
    }

    private void HandleControlPacket(object? sender, QuControlPacket packet)
    {
        if (QuPackets.IsInitialHandshakeResponse(packet, out var mixerUdpPort))
        {
            this.mixerUdpEndPoint = new IPEndPoint(IPAddress.Parse(host), mixerUdpPort);
            return;
        }
        var node = temporaryListeners.First;
        while (node is not null)
        {
            if (node.Value.HandlePacket(packet))
            {
                node.Value.Dispose();
                temporaryListeners.Remove(node);
            }
            node = node.Next;
        }
        foreach (var handler in packetHandlers)
        {
            handler(packet);
        }
    }

    private void HandleMeterPacket(object? sender, QuGeneralPacket packet)
    {
        if (packet.Type == QuPackets.InputMeterType)
        {
            var data = packet.Data;
            // TODO: Don't hard code this. (Where should we remember it?)
            var meters = new (ChannelId, MeterLevel)[32];
            for (int i = 0; i < meters.Length; i++)
            {
                var slice = data.Slice(i * 20, 20);
                meters[i] = (ChannelId.Input(i + 1), QuConversions.RawToMeterLevel(MemoryMarshal.Read<ushort>(slice.Slice(6, 2))));
            }
            receiver.ReceiveMeterLevels(meters);
        }
        else if (packet.Type == QuPackets.OutputMeterType)
        {
            var data = packet.Data;
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

    private void HandleFullDataPacket(QuGeneralPacket packet)
    {
        var data = new FullDataPacket(packet);
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

    private void HandleNetworkInformationPacket(QuGeneralPacket packet)
    {
        // Network information packet:
        // IP address (4 bytes)
        // Subnet mask (4 bytes)
        // Gateway (4 bytes)
        // DHCP enabled (1 byte)
        // Name (15 bytes)
        // ???? (4 bytes)
        var data = packet.Data;
        string name = Encoding.ASCII.GetString(data.Slice(13, 15));
        currentMixerInfo = currentMixerInfo with { Name = name };
        receiver.ReceiveMixerInfo(currentMixerInfo);
    }

    private void HandleVersionInformationPacket(QuGeneralPacket packet)
    {
        // Note: the model is probably encoded in the first two bytes, but that's hard to check...
        var data = packet.Data;
        int firmwareMajor = data[3];
        int firmwareMinor = data[2];
        ushort revision = MemoryMarshal.Read<ushort>(data.Slice(4, 2));
        currentMixerInfo = currentMixerInfo with { Model = "Qu-???", Version = $"{firmwareMajor}.{firmwareMinor} rev {revision}" };
        receiver.ReceiveMixerInfo(currentMixerInfo);
    }

    private void HandleValuePacket(QuValuePacket packet)
    {
        // logger.LogInformation("Received value packet: Client: {client}; Section: {section}; Address: {address}; Value:{value}", packet.ClientId, packet.Section, $"0x{packet.Address:x8}", packet.RawValue);

        // Fader and mute
        if (packet.Section == 4 && (packet.Address & 0xff_00_00_00) == 0x07_00_00_00)
        {
            int networkChannel = (packet.Address & 0x00_ff_00_00) >> 16;
            ChannelId? possibleChannelId = QuConversions.NetworkToChannelId(networkChannel);
            if (possibleChannelId is not ChannelId channelId)
            {
                return;
            }
            if ((packet.Address & 0xff) == 0x07)
            {
                if (channelId.IsInput)
                {
                    receiver.ReceiveFaderLevel(channelId, ChannelId.MainOutputLeft, QuConversions.RawToFaderLevel(packet.RawValue));
                }
                else
                {
                    receiver.ReceiveFaderLevel(channelId, QuConversions.RawToFaderLevel(packet.RawValue));
                }
            }
            else if ((packet.Address & 0xff) == 0x06)
            {
                receiver.ReceiveMuteStatus(channelId, packet.RawValue == 1);
            }
        }
        else if (packet.Section == 4 && (packet.Address & 0xff_ff) == 0x0c_0a)
        {
            int output = (packet.Address >> 24) + 1;
            ChannelId outputId =
                output < 5 ? ChannelId.Output(output)
                : output < 9 ? ChannelId.Output((output - 5) * 2 + 5)
                : output < 16 ? QuConversions.GroupChannelId((output - 9) * 2 + 1)
                : ChannelId.Output(output); // TODO: FX Send
            int input = ((packet.Address & 0xff_00_00) >> 16) + 1;
            receiver.ReceiveFaderLevel(ChannelId.Input(input), outputId, QuConversions.RawToFaderLevel(packet.RawValue));
        }
    }

    private void AddPacketHandlers()
    {
        AddGeneralHandlerForType(QuPackets.FullDataType, HandleFullDataPacket);
        AddGeneralHandlerForType(QuPackets.NetworkInformationType, HandleNetworkInformationPacket);
        AddGeneralHandlerForType(QuPackets.VersionInformationType, HandleVersionInformationPacket);
        packetHandlers.AddLast(packet =>
        {
            if (packet is QuValuePacket qvp)
            {
                HandleValuePacket(qvp);
            }
        });

        void AddGeneralHandlerForType(byte type, Action<QuGeneralPacket> action)
        {
            packetHandlers.AddLast(packet =>
            {
                if (packet is QuGeneralPacket general && general.Type == type)
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

    private class PacketListener : IDisposable
    {
        private readonly TaskCompletionSource<QuControlPacket> tcs;
        private readonly Func<QuControlPacket, bool> predicate;
        private readonly CancellationTokenRegistration ctr;

        internal Task<QuControlPacket> Task => tcs.Task;

        internal PacketListener(Func<QuControlPacket, bool> predicate, CancellationToken cancellationToken)
        {
            tcs = new TaskCompletionSource<QuControlPacket>();
            ctr = cancellationToken.Register(() => tcs.TrySetCanceled());
            this.predicate = predicate;
        }

        internal bool HandlePacket(QuControlPacket packet)
        {
            // If we've already cancelled the task, the listener is done.
            if (tcs.Task.IsCanceled)
            {
                return true;
            }
            if (!predicate(packet))
            {
                return false;
            }
            tcs.TrySetResult(packet);
            return true;
        }

        public void Dispose()
        {
            ctr.Unregister();
        }
    }
}
