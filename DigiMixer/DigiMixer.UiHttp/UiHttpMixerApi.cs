using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace DigiMixer.UiHttp;

using static UiMeters;

// TODO: make this thread-safe? Currently kinda hopes everything is on the dispatcher thread...
// TODO: Can we actually mute on a per output basis? Eek...
//       (Yes, we can - but turning on "aux send mute inheritance" makes it simpler.)
public class UiHttpMixerApi : IMixerApi
{
    private const string HttpPreamble = "GET /raw HTTP1.1\n\n";

    // Note: not static as it could be different for different mixers, even just Ui8 vs Ui12 vs Ui24
    private readonly Dictionary<string, Action<IMixerReceiver, UiMessage>> receiverActionsByAddress;

    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;

    private IUiClient sendingClient;
    private Task sendingClientReadTask;
    private IUiClient receivingClient;
    private Task receivingClientReadTask;
    private readonly DelegatingReceiver receiver = new();

    private MixerInfo currentMixerInfo;

    public UiHttpMixerApi(ILogger? logger, string host, int port = 80)
    {
        this.logger = logger ?? NullLogger.Instance;
        this.host = host;
        this.port = port;
        currentMixerInfo = MixerInfo.Empty;
        receiverActionsByAddress = BuildReceiverMap();
        sendingClient = IUiClient.Fake.Instance;
        receivingClient = IUiClient.Fake.Instance;
        sendingClientReadTask = Task.CompletedTask;
        receivingClientReadTask = Task.CompletedTask;
    }

    public void RegisterReceiver(IMixerReceiver receiver) =>
        this.receiver.RegisterReceiver(receiver);

    public async Task Connect(CancellationToken cancellationToken)
    {
        sendingClient.Dispose();
        receivingClient.Dispose();

        // Create two clients: one to send all changes to, and the other to
        // receive all information from. That way we have our own changes reflected back to us,
        // just like with other protocols.
        sendingClient = await CreateStreamClient(cancellationToken);
        receivingClient = await CreateStreamClient(cancellationToken);
        receivingClient.MessageReceived += ReceiveMessage;
        sendingClientReadTask = sendingClient.StartReading();
        receivingClientReadTask = receivingClient.StartReading();
        await CheckConnection(cancellationToken);
    }

    public async Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken)
    {
        var stereoChannelStatuses = new ConcurrentDictionary<ChannelId, bool>();

        EventHandler<UiMessage> handler = HandleMessage;
        try
        {
            sendingClient.MessageReceived += handler;
            await sendingClient.Send(UiMessage.InitMessage, cancellationToken);
            // Normally we get all the information in 100ms, but let's allow a bit longer.
            // Unfortunately the lines are unordered, so we can't really spot the end.
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }
        finally
        {
            sendingClient.MessageReceived -= handler;
        }

        var orderedChannelIds = stereoChannelStatuses.Keys.OrderBy(ch => ch.Value);
        var inputChannelIds = orderedChannelIds.Where(ch => ch.IsInput);
        var outputChannelIds = orderedChannelIds.Where(ch => ch.IsOutput)
            .Append(ChannelId.MainOutputLeft).Append(ChannelId.MainOutputRight);

        var stereoPairs = stereoChannelStatuses.Where(pair => pair.Value)
            .Select(pair => new StereoPair(pair.Key, pair.Key.WithValue(pair.Key.Value + 1), StereoFlags.FullyIndependent))
            // TODO: Check that we don't have independent faders/mutes
            .Append(new StereoPair(ChannelId.MainOutputLeft, ChannelId.MainOutputRight, StereoFlags.None));

        return new MixerChannelConfiguration(inputChannelIds, outputChannelIds, stereoPairs);

        void HandleMessage(object? sender, UiMessage message)
        {
            if (message.MessageType != UiMessage.SetDoubleMessageType ||
                message.Address is not string address ||
                !address.EndsWith(UiAddresses.StereoIndex, StringComparison.Ordinal))
            {
                return;
            }

            var maybeChannelId = UiAddresses.TryGetChannelId(address);
            if (maybeChannelId is ChannelId channelId)
            {
                // The value is "-1" for mono, "1" for "this is the right channel",
                // and "0" for "this is the left channel".
                stereoChannelStatuses[channelId] = message.Value == "0";
            }
            else
            {
                logger.LogWarning("Unexpected stereo index address: {address}", address);
            }
        }
    }

    private async Task<IUiClient> CreateStreamClient(CancellationToken initialCancellationToken)
    {
        var tcpClient = new TcpClient() { NoDelay = true };
        await tcpClient.ConnectAsync(host, port, initialCancellationToken);
        var stream = tcpClient.GetStream();
        byte[] preambleBytes = Encoding.ASCII.GetBytes(HttpPreamble);
        stream.Write(preambleBytes, 0, preambleBytes.Length);
        await ReadHttpResponseHeaders();
        return new UiStreamClient(logger, stream);

        async Task ReadHttpResponseHeaders()
        {
            Console.WriteLine("Reading HTTP response header");
            // Read a single byte at a time to avoid causing problems for the UiStreamClient.
            byte[] buffer = new byte[1];
            byte[] doubleLineBreak = { 0x0d, 0x0a, 0x0d, 0x0a };
            int doubleLineBreakIndex = 0;
            int totalBytesRead = 0;
            // If we've read this much data, we've missed it...
            while (totalBytesRead < 256)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, 1);
                if (bytesRead == 0)
                {
                    throw new InvalidDataException("Never reached the end of the HTTP headers");
                }
                if (buffer[0] == doubleLineBreak[doubleLineBreakIndex])
                {
                    doubleLineBreakIndex++;
                    if (doubleLineBreakIndex == doubleLineBreak.Length)
                    {
                        return;
                    }
                }
                else
                {
                    doubleLineBreakIndex = 0;
                }
                totalBytesRead++;
            }
            throw new InvalidDataException($"Read {totalBytesRead} bytes without reaching the end of HTTP headers");
        }
    }

    public async Task RequestAllData(IReadOnlyList<ChannelId> channelIds)
    {
        currentMixerInfo = MixerInfo.Empty;
        // Note: this call *does* need to send a message to receivingClient
        if (receivingClient is not null)
        {
            await receivingClient.Send(UiMessage.InitMessage);
        }
    }

    public TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(3);

    public Task SendKeepAlive() => SendKeepAlive(default);

    private async Task SendKeepAlive(CancellationToken cancellationToken)
    {
        // Note: this call *does* need to send a message to receivingClient
        await sendingClient.Send(UiMessage.AliveMessage, cancellationToken);
        await receivingClient.Send(UiMessage.AliveMessage, cancellationToken);
    }

    public async Task<bool> CheckConnection(CancellationToken cancellationToken)
    {
        // Propagate any existing client failures.
        if (sendingClientReadTask?.IsFaulted == true)
        {
            return false;
        }
        if (receivingClientReadTask?.IsFaulted == true)
        {
            return false;
        }

        // TODO: Fail if we haven't seen any data recently?
        await SendKeepAlive(cancellationToken);

        return true;
    }

    public Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level) =>
        sendingClient.Send(UiMessage.CreateSetMessage(UiAddresses.GetFaderAddress(inputId, outputId), FromFaderLevel(level))) ?? Task.CompletedTask;

    public Task SetFaderLevel(ChannelId outputId, FaderLevel level) =>
        sendingClient.Send(UiMessage.CreateSetMessage(UiAddresses.GetFaderAddress(outputId), FromFaderLevel(level))) ?? Task.CompletedTask;

    public Task SetMuted(ChannelId inputId, bool muted) =>
        sendingClient.Send(UiMessage.CreateSetMessage(UiAddresses.GetMuteAddress(inputId), muted)) ?? Task.CompletedTask;

    private void ReceiveMessage(object? sender, UiMessage message)
    {
        /*
        if (message.MessageType != "RTA" && message.MessageType != "VU2")
        {
            Console.WriteLine($"Received message:");
            Console.WriteLine($"Type: {message.MessageType}");
            Console.WriteLine($"Address: {message.Address}");
            Console.WriteLine($"Value: {message.Value}");
            Console.WriteLine();
        }*/
        if (receiver.IsEmpty)
        {
            return;
        }
        if (message.MessageType == UiMessage.MeterType && message.Value is string base64)
        {
            ProcessMeters(base64);
            return;
        }
        if (message.Address is not string address)
        {
            return;
        }
        if (!receiverActionsByAddress.TryGetValue(address, out var action))
        {
            return;
        }
        action(receiver, message);
    }

    private void ProcessMeters(string base64)
    {
        // TODO: Implement a more efficient version that converts into an existing buffer
        var rawData = Convert.FromBase64String(base64);

        int inputs = rawData[Header.Inputs];
        int media = rawData[Header.Media];
        int subgroups = rawData[Header.Subgroups];
        int fx = rawData[Header.Fx];
        int aux = rawData[Header.Aux];
        int mains = rawData[Header.Mains];
        int linesIn = rawData[Header.LinesIn];

        // Just check that it looks okay...
        if (rawData.Length != Header.Length +
            (inputs + media + linesIn) * InputsMediaLinesIn.Length +
            (subgroups + fx) * SubgroupsFx.Length +
            (aux + mains) * AuxMains.Length)
        {
            // TODO: Some kind of warning?
            // (Maybe the receiver should have a warning method...)
            return;
        }

        var levels = new (ChannelId, MeterLevel)[inputs + aux + 2];
        int index = 0;

        int inputsStart = Header.Length;
        for (int i = 0; i < inputs; i++)
        {
            var rawLevel = rawData[inputsStart + i * InputsMediaLinesIn.Length + InputsMediaLinesIn.Pre];
            var meterLevel = ToMeterLevel(rawLevel);
            levels[index++] = (ChannelId.Input(i + 1), meterLevel);
        }

        int auxStart = Header.Length + (inputs + media) * InputsMediaLinesIn.Length + (subgroups + fx) * SubgroupsFx.Length;
        for (int i = 0; i < aux; i++)
        {
            var rawLevel = rawData[auxStart + i * AuxMains.Length + AuxMains.PostFader];
            var meterLevel = ToMeterLevel(rawLevel);
            levels[index++] = (ChannelId.Output(i + 1), meterLevel);
        }

        // TODO: Validate that we have 2 main outputs?
        int mainStart = auxStart + aux * AuxMains.Length;
        for (int i = 0; i < 2; i++)
        {
            var rawLevel = rawData[mainStart + i * AuxMains.Length + AuxMains.PostFader];
            var meterLevel = ToMeterLevel(rawLevel);
            levels[index++] = (ChannelId.Output(ChannelId.MainOutputLeft.Value + i), meterLevel);
        }
        receiver.ReceiveMeterLevels(levels);

        MeterLevel ToMeterLevel(byte rawValue)
        {
            // 240 is actually the highest value we see, but let's make sure that's actually the case...
            int normalized = Math.Min((int) rawValue, 240);
            return new MeterLevel(normalized / 240.0);
        }
    }

    private Dictionary<string, Action<IMixerReceiver, UiMessage>> BuildReceiverMap()
    {
        var ret = new Dictionary<string, Action<IMixerReceiver, UiMessage>>();

        var inputs = Enumerable.Range(1, 22).Select(ChannelId.Input).Append(UiAddresses.PlayerLeft).Append(UiAddresses.PlayerRight).Append(UiAddresses.LineInLeft).Append(UiAddresses.LineInRight);
        // Note: no MainOutputRight here as it doesn't have a separate address, so we don't need a separate receiver map.
        var outputs = Enumerable.Range(1, 8).Select(ChannelId.Output).Append(ChannelId.MainOutputLeft);

        // We don't know what order we'll get firmware and model in.
        ret[UiAddresses.Model] = (receiver, message) =>
        {
            currentMixerInfo = currentMixerInfo with { Model = message!.Value };
            receiver.ReceiveMixerInfo(currentMixerInfo);
        };
        ret[UiAddresses.Firmware] = (receiver, message) =>
        {
            currentMixerInfo = currentMixerInfo with { Version = message!.Value };
            receiver.ReceiveMixerInfo(currentMixerInfo);
        };

        // TODO: Populate this on-demand? Would avoid assumptions about the number of inputs/outputs...
        foreach (var input in inputs)
        {
            ret[UiAddresses.GetNameAddress(input)] = (receiver, message) => receiver.ReceiveChannelName(input, message.Value!);
            ret[UiAddresses.GetMuteAddress(input)] = (receiver, message) => receiver.ReceiveMuteStatus(input, message.Value == "1");
        }

        foreach (var input in inputs)
        {
            foreach (var output in outputs)
            {
                ret[UiAddresses.GetFaderAddress(input, output)] = (receiver, message) => receiver.ReceiveFaderLevel(input, output, ToFaderLevel(message.DoubleValue));
            }
        }

        foreach (var output in outputs)
        {
            ret[UiAddresses.GetNameAddress(output)] = (receiver, message) => receiver.ReceiveChannelName(output, message.Value!);
            ret[UiAddresses.GetMuteAddress(output)] = (receiver, message) => receiver.ReceiveMuteStatus(output, message.BoolValue);
            ret[UiAddresses.GetFaderAddress(output)] = (receiver, message) => receiver.ReceiveFaderLevel(output, ToFaderLevel(message.DoubleValue));
        }

        return ret;
    }

    private static double FromFaderLevel(FaderLevel level) => level.Value / (float) FaderLevel.MaxValue;

    private static FaderLevel ToFaderLevel(double value) => new FaderLevel((int) (value * FaderLevel.MaxValue));

    public void Dispose()
    {
        sendingClient.Dispose();
        receivingClient.Dispose();

        sendingClient = IUiClient.Fake.Instance;
        receivingClient = IUiClient.Fake.Instance;
    }
}
