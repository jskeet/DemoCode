using DigiMixer.Core;
using DigiMixer.Diagnostics;
using DigiMixer.DmSeries.Core;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography;

namespace DigiMixer.DmSeries.Tools;

public class StartClient : Tool
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

        //var message1 = new DmMessage("MPRO", 0x01010102, [propertySegment, new DmBinarySegment([0x80])]);
        //await Send(message1);
        /*
        message2.DisplayStructure("=>");
        await client.SendAsync(message2, default);

        message3.DisplayStructure("=>");
        await client.SendAsync(message3, default);
        */
        await Task.Delay(1000);

        var message2 = new DmMessage("MMIX", 0x01010102, [new DmTextSegment("Mixing"), new DmBinarySegment([0x80])]);
        await Send(message2);

        await Task.Delay(1000);

        var fader1Message = new DmMessage("MMIX", 0x01110108,
            [new DmBinarySegment([0x00]),
                new DmTextSegment("Mixing"),
                new DmTextSegment("Mixing"),
                new DmUInt16Segment([0x03]),
                new DmUInt32Segment([0, 0xd, 0]),
                new DmUInt32Segment([0, 0, 0]),
                new DmUInt32Segment([0xa0]),
                new DmInt32Segment([-1365])]);

        await Send(fader1Message);

        //var meterClient = new MeterClient();
        //meterClient.Start();
        // await Send(DmMessages.MeterRequest);

        while (true)
        {
            await Send(DmMessages.KeepAlive);
            await Task.Delay(1000);
        }

        async Task HandleMessage(DmMessage message, CancellationToken cancellationToken)
        {
            message.DisplayStructure("<=");
            if (message.Flags == 0x01100104 && message.Segments is [DmBinarySegment seg1, DmTextSegment seg2, DmBinarySegment seg3, DmBinarySegment seg4] &&
                seg1.Data is [0x00] && seg3.Data.Length == 16 && seg4.Data.Length == 16)
            {
                var response = new DmMessage(message.Type, 0x01100104, [seg1, seg2, seg4, new DmBinarySegment(new byte[16])]);
                await Send(response);

                await Send(new DmMessage(message.Type, 0x01040100, []));
                await Send(new DmMessage(message.Type, 0x01041000, []));
            }
            else if (message.Flags == 0x01140109)
            {
                var binary1 = (DmBinarySegment) message.Segments[7];
                var binary2 = (DmBinarySegment) message.Segments[8];
                var hash1 = MD5.HashData(binary1.Data);
                var hash2 = MD5.HashData(binary2.Data);
                Console.WriteLine($"Hash 1 for {message.Type}: {Formatting.ToHex(hash1)}");
                Console.WriteLine($"Hash 2 for {message.Type}: {Formatting.ToHex(hash2)}");
            }
        }

        async Task Send(DmMessage message)
        {
            message.DisplayStructure("=>");
            await client.SendAsync(message, default);
        }
    }

    private class MeterClient : UdpControllerBase
    {
        public MeterClient() : base(NullLogger.Instance, 50272)
        {
        }

        protected override void ProcessData(ReadOnlySpan<byte> data)
        {
            Console.WriteLine($"Received meter message length {data.Length}");
        }
    }
}
