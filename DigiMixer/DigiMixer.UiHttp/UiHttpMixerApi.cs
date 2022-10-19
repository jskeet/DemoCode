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

    private UiStreamClient? sendingClient;
    private Task? sendingClientReadTask;
    private UiStreamClient? receivingClient;
    private Task? receivingClientReadTask;
    private readonly ConcurrentBag<IMixerReceiver> receivers = new ConcurrentBag<IMixerReceiver>();

    // We get these separately, so when we've seen both, we respond with "mixer info received".
    private string? model;
    private string? firmware;

    public UiHttpMixerApi(ILogger? logger, string host, int port)
    {
        this.logger = logger ?? NullLogger.Instance;
        this.host = host;
        this.port = port;
        receiverActionsByAddress = BuildReceiverMap();
    }

    public void RegisterReceiver(IMixerReceiver receiver) =>
        receivers.Add(receiver);

    public async Task Connect()
    {
        sendingClient?.Dispose();
        receivingClient?.Dispose();

        // Create two clients: one to send all changes to, and the other to
        // receive all information from. That way we have our own changes reflected back to us,
        // just like with other protocols.
        sendingClient = await CreateStreamClient();
        receivingClient = await CreateStreamClient();
        receivingClient.MessageReceived += ReceiveMessage;
        sendingClientReadTask = sendingClient.StartReading();
        receivingClientReadTask = receivingClient.StartReading();
    }

    private async Task<UiStreamClient> CreateStreamClient()
    {
        var tcpClient = new TcpClient() { NoDelay = true };
        await tcpClient.ConnectAsync(host, port);
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

    public async Task RequestAllData(IReadOnlyList<InputChannelId> inputChannels, IReadOnlyList<OutputChannelId> outputChannels)
    {
        model = null;
        firmware = null;
        // Note: this call *does* need to send a message to receivingClient
        if (receivingClient is not null)
        {
            await receivingClient.Send(UiMessage.InitMessage);
        }
    }

    public async Task SendKeepAlive()
    {
        // Note: this call *does* need to send a message to receivingClient
        await (sendingClient?.Send(UiMessage.AliveMessage) ?? Task.CompletedTask);
        await (receivingClient?.Send(UiMessage.AliveMessage) ?? Task.CompletedTask);
    }

    public Task SetFaderLevel(InputChannelId inputId, OutputChannelId outputId, FaderLevel level) =>
        sendingClient?.Send(UiMessage.CreateSetMessage(UiAddresses.GetFaderAddress(inputId, outputId), FromFaderLevel(level))) ?? Task.CompletedTask;

    public Task SetFaderLevel(OutputChannelId outputId, FaderLevel level) =>
        sendingClient?.Send(UiMessage.CreateSetMessage(UiAddresses.GetFaderAddress(outputId), FromFaderLevel(level))) ?? Task.CompletedTask;

    public Task SetMuted(InputChannelId inputId, bool muted) =>
        sendingClient?.Send(UiMessage.CreateSetMessage(UiAddresses.GetMuteAddress(inputId), muted)) ?? Task.CompletedTask;

    // TODO: What about stereo? Seems to only mute the left part.
    public Task SetMuted(OutputChannelId outputId, bool muted) =>
        sendingClient?.Send(UiMessage.CreateSetMessage(UiAddresses.GetMuteAddress(outputId), muted)) ?? Task.CompletedTask;

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
        if (receivers.IsEmpty)
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
        foreach (var receiver in receivers)
        {
            action(receiver, message);
        }
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

        int inputsStart = Header.Length;
        for (int i = 0;  i < inputs; i++)
        {
            var rawLevel = rawData[inputsStart + i * InputsMediaLinesIn.Length + InputsMediaLinesIn.Pre];
            var meterLevel = ToMeterLevel(rawLevel);
            foreach (var receiver in receivers)
            {
                receiver.ReceiveMeterLevel(new InputChannelId(i + 1), meterLevel);
            }
        }

        int auxStart = Header.Length + (inputs + media) * InputsMediaLinesIn.Length + (subgroups + fx) * SubgroupsFx.Length;
        for (int i = 0; i < aux; i++)
        {
            var rawLevel = rawData[auxStart + i * AuxMains.Length + AuxMains.PostFader];
            var meterLevel = ToMeterLevel(rawLevel);
            foreach (var receiver in receivers)
            {
                receiver.ReceiveMeterLevel(new OutputChannelId(i + 1), meterLevel);
            }
        }

        // TODO: Validate that we have 2 main outputs?
        int mainStart = auxStart + aux * AuxMains.Length;
        for (int i = 0; i < 2; i++)
        {
            var rawLevel = rawData[mainStart + i * AuxMains.Length + AuxMains.PostFader];
            var meterLevel = ToMeterLevel(rawLevel);
            foreach (var receiver in receivers)
            {
                receiver.ReceiveMeterLevel(new OutputChannelId(UiAddresses.MainOutput.Value + i), meterLevel);
            }
        }

        MeterLevel ToMeterLevel(byte rawValue)
        {
            // 240 is actually the highest value we see, but let's make sure that's actually the case...
            int normalized = Math.Min((int) rawValue, 240);

            // -240 to 0.
            int signed = rawValue - 240;
            // This is massively wrong, but it's a start.
            return new MeterLevel(signed / 3.0);
        }
    }

    private Dictionary<string, Action<IMixerReceiver, UiMessage>> BuildReceiverMap()
    {
        var ret = new Dictionary<string, Action<IMixerReceiver, UiMessage>>();

        // TODO: Check this! USB?
        var inputs = Enumerable.Range(1, 22).Select(id => new InputChannelId(id));
        var outputs = Enumerable.Range(1, 8).Select(id => new OutputChannelId(id)).Append(UiAddresses.MainOutput);

        // We don't know what order we'll get firmware and model in.
        ret[UiAddresses.Model] = (receiver, message) =>
        {
            model = message!.Value;
            MaybeReceiveMixerInfo(receiver);
        };
        ret[UiAddresses.Firmware] = (receiver, message) =>
        {
            firmware = message.Value!;
            MaybeReceiveMixerInfo(receiver);
        };
        void MaybeReceiveMixerInfo(IMixerReceiver receiver)
        {
            if (model is not null && firmware is not null)
            {
                receiver.ReceiveMixerInfo(new MixerInfo(model, null, firmware));
            }
        }

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

    public void Dispose() => sendingClient?.Dispose();
}
