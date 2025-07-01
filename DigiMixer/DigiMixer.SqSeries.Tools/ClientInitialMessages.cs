using AllenAndHeath.Core;
using DigiMixer.AllenAndHeath.Core;
using DigiMixer.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.SqSeries.Tools;

public class ClientInitialMessages : Tool
{
    public override async Task<int> Execute()
    {
        var meterClient = new AHMeterClient(NullLogger.Instance);
        var controlClient = new AHControlClient(NullLogger.Instance, "192.168.1.56", 51326);       

        controlClient.MessageReceived += LogMessage;
        await controlClient.Connect(default);
        controlClient.Start();

        await SendAsync(new SqUdpHandshakeMessage(meterClient.LocalUdpPort));
        await Task.Delay(500);
        await SendAsync(new SqVersionRequestMessage());
        await Task.Delay(500);
        await SendAsync(new SqClientInitRequestMessage());
        await Task.Delay(500);
        await SendAsync(new SqSimpleRequestMessage(SqMessageType.FullDataRequest));
        await Task.Delay(500);
        await SendAsync(new SqSimpleRequestMessage(SqMessageType.Type15Request));
        await Task.Delay(500);
        await SendAsync(new SqSimpleRequestMessage(SqMessageType.Type17Request));
        await Task.Delay(5000);

        return 0;

        Task SendAsync (SqMessage message) => controlClient.SendAsync(message.RawMessage, default);
    }

    void LogMessage(object? sender, AHRawMessage message)
    {
        var sqMessage = SqMessage.FromRawMessage(message);
        Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff}: {sqMessage}");
    }
}
