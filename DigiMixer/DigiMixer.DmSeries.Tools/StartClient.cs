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
        client.MessageReceived += (sender, message) => message.DisplayStructure("<=");
        await client.Connect(default);
        client.Start();

        var propertySegment = new DmTextSegment("Property");

        var message1 = new DmMessage("MPRO", 0x01010102, [propertySegment, new DmBinarySegment([0x80])]);
        var message2 = new DmMessage("MPRO", 0x01100104,
            [new DmBinarySegment([0x00]),
                propertySegment,
                DmBinarySegment.FromHex("3A 7C 8D 4C 85 F8 9F 1E AA 83 4F 96 63 0C EC 3D"),
                DmBinarySegment.FromHex("20 DC D8 75 45 50 DD 5C B1 0D 89 00 AE 71 4A 04")]);
        var message3 = new DmMessage("MPRO", 0x01040100, []);

        message1.DisplayStructure("=>");
        await client.SendAsync(message1, default);

        message2.DisplayStructure("=>");
        await client.SendAsync(message2, default);

        message3.DisplayStructure("=>");
        await client.SendAsync(message3, default);
        while (true)
        {
            await Task.Delay(1000);
        }
    }
}
