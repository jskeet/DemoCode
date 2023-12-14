using DigiMixer.Diagnostics;
using DigiMixer.DmSeries.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.DmSeries.Tools;

public class WatchMixerMessages : Tool
{
    private const string Host = "192.168.1.86";
    private const int Port = 50368;

    public override async Task<int> Execute()
    {
        // Slightly ugly, but avoids the use of client being problematic in HandleMessage.
        DmClient client = null!;
        client = new DmClient(NullLogger.Instance, Host, Port, HandleMessage);
        await client.Connect(default);
        client.Start();

        var propertySegment = new DmTextSegment("Property");

        var message1 = new DmMessage("MPRO", 0x01010102, [propertySegment, new DmBinarySegment([0x80])]);
        await Send(message1);


        await Send(new DmMessage("MMIX", 0x01010102, [new DmTextSegment("Mixing"), new DmBinarySegment([0x80])]));

        await Task.Delay(1000);
        await Send(DmMessages.MeterRequest);

        while (true)
        {
            await Send(DmMessages.KeepAlive);
            await Task.Delay(1000);
        }

        async Task HandleMessage(DmMessage message, CancellationToken cancellationToken)
        {
            // Don't log keepalive messages
            if (message.Type == "EEVT" && message.Segments is [_, _, DmTextSegment { Text: "KeepAlive" }, _])
            {
                return;
            }
            message.DisplayStructure("<=");
            if (message.Flags == 0x01100104 && message.Segments is [DmBinarySegment seg1, DmTextSegment seg2, DmBinarySegment seg3, DmBinarySegment seg4] &&
                seg1.Data is [0x00] && seg3.Data.Length == 16 && seg4.Data.Length == 16)
            {
                var response = new DmMessage(message.Type, 0x01100104, [seg1, seg2, seg4, new DmBinarySegment(new byte[16])]);
                await Send(response);

                await Send(new DmMessage(message.Type, 0x01040100, []));
                await Send(new DmMessage(message.Type, 0x01041000, []));
            }
        }

        async Task Send(DmMessage message)
        {
            await client.SendAsync(message, default);
        }
    }
}
