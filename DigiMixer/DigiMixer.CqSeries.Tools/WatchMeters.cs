using DigiMixer.CqSeries.Core;
using DigiMixer.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace DigiMixer.CqSeries.Tools;

public class WatchMeters : Tool
{
    // Assume the address...
    private const string Address = "192.168.1.85";
    private const int Port = 51326;

    public async override Task<int> Execute()
    {
        var meterClient = new CqMeterClient(NullLogger.Instance /* loggingFactory.CreateLogger("Meter")*/);
        var controlClient = new CqControlClient(NullLogger.Instance/*loggingFactory.CreateLogger("Control")*/, Address, Port);

        IPEndPoint? mixerUdpEndpoint = null;
        controlClient.MessageReceived += (sender, rawMessage) =>
        {
            var message = CqMessage.FromRawMessage(rawMessage);
            if (message is CqUdpHandshakeMessage handshake)
            {
                mixerUdpEndpoint = new IPEndPoint(IPAddress.Parse(Address), handshake.UdpPort);
            }
        };
        meterClient.MessageReceived += LogMeterMessage;

        meterClient.Start();
        await controlClient.Connect(default);
        controlClient.Start();

        var handshake = new CqUdpHandshakeMessage(meterClient.LocalUdpPort);
        await SendAsync(handshake);

        await Task.Delay(100);

        await SendAsync(new CqVersionRequestMessage());
        await Task.Delay(100);
        await SendAsync(new CqUnknownMessage(new(CqMessageFormat.VariableLength, CqMessageType.ClientInitRequest, [0x02, 0x00])));


        while (true)
        {
            await Task.Delay(3000);
            if (mixerUdpEndpoint is IPEndPoint target)
            {
                await meterClient.SendAsync(new CqKeepAliveMessage().RawMessage, target, default);
            }
        }

        async Task SendAsync(CqMessage message) => await controlClient.SendAsync(message.RawMessage, default);

        void LogMeterMessage(object? sender, CqRawMessage rawMessage)
        {
            var message = CqMessage.FromRawMessage(rawMessage);
            //Console.WriteLine($"{message.Type}: {message.Data.Length}");
            if (message.Type == CqMessageType.OutputMeters)
            {
                Console.Clear();
                var lines = Hex.ConvertAndSplit(message.Data, 16, 8);
                foreach (var line in lines)
                {
                    Console.WriteLine(line);
                }
            }
        }
    }
}
