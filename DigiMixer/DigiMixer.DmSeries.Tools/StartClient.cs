using DigiMixer.Diagnostics;
using DigiMixer.DmSeries.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.DmSeries.Tools;

public class StartClient : Tool
{
    private const string Host = "192.168.1.86";
    private const int Port = 50368;

    public override async Task<int> Execute()
    {
        var client = new DmClient(NullLogger.Instance, Host, Port);
        client.MessageReceived += (sender, message) => Console.WriteLine(message);
        await client.Connect(default);
        client.Start();

        var message1 = new DmMproMessage(new DmMproMessage.Chunk([0x01, 0x01, 0x01, 0x02, 0x31, 0x00, 0x00, 0x00, 0x09, 0x50, 0x72, 0x6f, 0x70, 0x65, 0x72, 0x74, 0x79, 0x00, 0x11, 0x00, 0x00, 0x00, 0x01, 0x80]));
        await client.SendAsync(message1.RawMessage, default);

        while (true)
        {
            await Task.Delay(1000);
        }
    }
}