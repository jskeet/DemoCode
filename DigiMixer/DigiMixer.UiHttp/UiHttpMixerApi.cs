using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace DigiMixer.UiHttp;

// TODO: make this thread-safe? Currently kinda hopes everything is on the dispatcher thread...
// TODO: Can we actually mute on a per output basis? Eek...
// TODO: Use Init to get information? Hmm. Fetch all info instead of individual request messages?
public class UiHttpMixerApi : IMixerApi
{
    private const string HttpPreamble = "GET /raw HTTP1.1\n\n";

    // Note: not static as it could be different for different mixers, even just Ui8 vs Ui12 vs Ui24
    private readonly Dictionary<string, Action<IMixerReceiver, UiMessage>> receiverActionsByAddress;

    private readonly string host;
    private readonly int port;

    private UiStreamClient? client;
    private Task? readingTask;
    private readonly ConcurrentBag<IMixerReceiver> receivers = new ConcurrentBag<IMixerReceiver>();

    public UiHttpMixerApi(string host, int port)
    {
        this.host = host;
        this.port = port;
        receiverActionsByAddress = BuildReceiverMap();
    }

    public async Task ConnectAsync()
    {
        client?.Dispose();

        var tcpClient = new TcpClient() { NoDelay = true };
        await tcpClient.ConnectAsync(host, port);
        var stream = tcpClient.GetStream();
        byte[] preambleBytes = Encoding.ASCII.GetBytes(HttpPreamble);
        stream.Write(preambleBytes, 0, preambleBytes.Length);
        await ReadHttpResponseHeaders();
        client = new UiStreamClient(stream);
        client.MessageReceived += ReceiveMessage;
        readingTask = client.StartReading();
        return;

        async Task ReadHttpResponseHeaders()
        {
            // Read a single byte at a time to avoid causing problems for the UiStreamClient.
            byte[] buffer = new byte[1];
            bool lastByteWasLineFeed = false;
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, 1);
                if (bytesRead == 0)
                {
                    throw new InvalidDataException("Never reached the end of the HTTP headers");
                }
                bool currentByteIsLineFeed = buffer[1] == '\n';
                if (currentByteIsLineFeed && lastByteWasLineFeed)
                {
                    return;
                }
                lastByteWasLineFeed = currentByteIsLineFeed;
            }
        }
    }

    private void ReceiveMessage(object? sender, UiMessage message)
    {
        if (message.MessageType != "RTA" && message.MessageType != "VU2")
        {
            Console.WriteLine($"Received message:");
            Console.WriteLine($"Type: {message.MessageType}");
            Console.WriteLine($"Address: {message.Address}");
            Console.WriteLine($"Value: {message.Value}");
            Console.WriteLine();
        }
        if (receivers.IsEmpty)
        {
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

    public void RegisterReceiver(IMixerReceiver receiver) =>
        receivers.Add(receiver);

    public Task SetFaderLevel(InputChannelId inputId, OutputChannelId outputId, FaderLevel level) =>
        client?.Send(UiMessage.CreateSetMessage(UiAddresses.GetFaderAddress(inputId, outputId), FromFaderLevel(level))) ?? Task.CompletedTask;

    public Task SetFaderLevel(OutputChannelId outputId, FaderLevel level) =>
        client?.Send(UiMessage.CreateSetMessage(UiAddresses.GetFaderAddress(outputId), FromFaderLevel(level))) ?? Task.CompletedTask;

    public Task SetMuted(InputChannelId inputId, bool muted) =>
        client?.Send(UiMessage.CreateSetMessage(UiAddresses.GetMuteAddress(inputId), muted)) ?? Task.CompletedTask;

    public Task SetMuted(OutputChannelId outputId, bool muted) =>
        client?.Send(UiMessage.CreateSetMessage(UiAddresses.GetMuteAddress(outputId), muted)) ?? Task.CompletedTask;

    public Task RequestMixerInfo() =>
        client?.Send(UiMessage.InfoMessage) ?? Task.CompletedTask;

    public Task RequestName(InputChannelId inputId)
    {
        return Task.CompletedTask;
    }

    public Task RequestName(OutputChannelId outputId)
    {
        return Task.CompletedTask;
    }

    public Task RequestMuteStatus(InputChannelId inputId)
    {
        return Task.CompletedTask;
    }

    public Task RequestMuteStatus(OutputChannelId outputId)
    {
        return Task.CompletedTask;
    }

    public Task RequestFaderLevel(InputChannelId inputId, OutputChannelId outputId)
    {
        return Task.CompletedTask;
    }

    public Task RequestFaderLevel(OutputChannelId outputId)
    {
        return Task.CompletedTask;
    }

    public Task RequestChannelUpdates()
    {
        return Task.CompletedTask;
    }

    public Task RequestMeterUpdates()
    {
        return Task.CompletedTask;
    }

    private static Dictionary<string, Action<IMixerReceiver, UiMessage>> BuildReceiverMap()
    {
        var ret = new Dictionary<string, Action<IMixerReceiver, UiMessage>>();

        // TODO: Check this! USB?
        var inputs = Enumerable.Range(1, 22).Select(id => new InputChannelId(id));
        var outputs = Enumerable.Range(1, 8).Select(id => new OutputChannelId(id)).Append(UiAddresses.MainOutput);

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

    public void Dispose() => client.Dispose();

    public Task SendAlive()
    {
        return client.Send(UiMessage.AliveMessage);
    }
}
