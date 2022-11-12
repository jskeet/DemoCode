using DigiMixer.Mackie;
using Microsoft.Extensions.Logging;
using System.Text;

var factory = LoggerFactory.Create(builder => builder.AddConsole().AddSystemdConsole(options => { options.UseUtcTimestamp = true; options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'"; })
    .SetMinimumLevel(LogLevel.Trace));
var logger = factory.CreateLogger("MackieClient");

var controller = new MackieController(logger, "192.168.1.59", 50001);

// TODO: Check the request for GeneralInfo? Not sure what else we could return.
controller.MapCommand(MackieCommand.ClientHandshake, _ => new byte[] { 0x10, 0x40, 0xf0, 0x1d, 0xbc, 0xa2, 0x88, 0x1c });
controller.MapCommand(MackieCommand.GeneralInfo, _ => new byte[] { 0, 0, 0, 2, 0, 0, 0x40, 0 });
controller.MapCommand(MackieCommand.ChannelInfoControl, packet => new MackiePacketBody(packet.Body.Data.Slice(0, 4)));
controller.MapCommandAction(MackieCommand.ChannelValues, packet =>
{
    Console.WriteLine("Got channel info");
});
controller.MapCommandAction(MackieCommand.ChannelName, packet =>
{
    Console.WriteLine("Received channel names:");
    var body = packet.Body.InSequentialOrder();
    var names = body.Data.Slice(8);
    int lastStart = 0;
    int count = 0;
    for (int index = 0; index < names.Length; index++)
    {
        if (names[index] == 0)
        {
            string name = Encoding.ASCII.GetString(names.Slice(lastStart, index - lastStart));
            Console.WriteLine($"  '{name}'");
            lastStart = index + 1;
            count++;
        }
    }
    // We seem to get one more than expected. Hmm.
    Console.WriteLine($"Names received: {count}");
});

controller.MapBroadcastAction(packet =>
{
    Console.WriteLine("Received broadcast");
});


var task = controller.Start();

await controller.SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty);
await controller.SendRequest(MackieCommand.ChannelInfoControl, new byte[8]);
await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 2 });
var info = await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 0x12 });
Console.WriteLine($"Received info: {Encoding.ASCII.GetString(info.Body.InSequentialOrder().Data)}");
await controller.SendRequest(MackieCommand.ChannelInfoControl, new byte[] { 0, 0, 0, 6 });

var meterLayout = Enumerable.Range(1, 221).SelectMany(i => new byte[] { 0, 0, 0, (byte) i });
await controller.SendRequest(MackieCommand.MeterLayout, new byte[] { 0, 0, 0, 1 }.Concat(meterLayout).ToArray());
await controller.SendRequest(MackieCommand.BroadcastControl, new byte[] { 0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x01, 0x00, 0x00, 0x5a, 0x00, 0x01 });



for (int i = 0; i < 5; i++)
{
    await Task.Delay(1000);
    await controller.SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty);
}




/*
















client.PacketReceived += HandlePacket;

var task = client.StartReceiving();

await client.SendRequest(MackieCommand.KeepAlive, null);
await Task.Delay(250);
await client.SendRequest((MackieCommand) 6, new byte[8]);
await Task.Delay(250);
//await client.SendRequest((MackieCommand) 3, null);
//await Task.Delay(250);

await client.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 2 });
await Task.Delay(250);
//await client.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 3 });
//await Task.Delay(250);
//await client.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 4 });
//await Task.Delay(250);
//await client.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 0x12 });
//await Task.Delay(250);
//await client.SendRequest(MackieCommand.FirmwareInfo, null);
//await Task.Delay(250);

//await client.SendRequest((MackieCommand) 6, new byte[] { 0, 0, 0, 6 });
//await Task.Delay(250);

//await client.SendRequest((MackieCommand) 22, null);
// Don't know what this data means, unfortunately... but it's vital to get the mixer to broadcast.
// It's 888 bytes, and each broadcast is 892 bytes. Taking just the first 800 bytes,
// we get broadcasts of 804. So some sort of layout?
// It's always just values 1 - 221 with a leading 1, but in a random order. So can we just send 1, 2, 3, 4, 5...?
//await client.SendRequest((MackieCommand) 22, File.ReadAllBytes("command-21.dat").ToArray());
//await client.SendRequest((MackieCommand) 22, ParseHex("00 00 00 01 00 00 00 01 00 00 00 11 00 00 00 12 00 00 00 13 00 00 00 14 00 00 00 15 00 00 00 16 00 00 00 17 00 00 00 18 00 00 00 19 00 00 00 1a 00 00 00 1b 00 00 00 1c 00 00 00 1d 00 00 00 1e 00 00 00 1f 00 00 00 20"));// File.ReadAllBytes("command-21.dat").ToArray());
//await client.SendRequest((MackieCommand) 22, ParseHex("00 00 00 01 00 00 00 09"));


var allValues = Enumerable.Range(1, 221).SelectMany(i => new byte[] { 0, 0, 0, (byte) i });
await client.SendRequest(MackieCommand.MeterLayout, new byte[] { 0, 0, 0, 1 }.Concat(meterLayout).ToArray());

//await Task.Delay(250);
//await client.SendRequest((MackieCommand) 9, new byte[] { 0, 0, 0, 5 });

await Task.Delay(3000);
await client.SendRequest((MackieCommand) 0x15, new byte[] { 0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x01, 0x00, 0x00, 0x5a, 0x00, 0x01 });

//await Task.Delay(3000);
//await client.SendRequest((MackieCommand) 0x15, new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00 });

await Task.Delay(3000);

while (true)
{
    await Task.Delay(1000);
    await client.SendRequest(MackieCommand.KeepAlive, null);
}

// TODO: Avoid async void?
async void HandlePacket(object? sender, MackiePacket packet)
{
    if (packet.Type == MackiePacketType.Request)
    {
        if (packet.Command == MackieCommand.ChannelValues && packet.Body.Length < 40)
        {
            //Console.WriteLine($"Received values: {BitConverter.ToString(packet.Body.ToArray())}");
        }

        var responseBody = packet.Command switch
        {
            (MackieCommand) 3 => new byte[] { 0x10, 0x40, 0xf0, 0x1d, 0xbc, 0xa2, 0x88, 0x1c },
            (MackieCommand) 0x0e when packet.Body[2] == 2 =>
                new byte[] { 0, 0, 0, 2, 0, 0, 0x40, 0 },
            (MackieCommand) 0x06 => packet.Body[0..4].ToArray(),
            MackieCommand.GeneralInfo => new byte[] { 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x40, 0x00 },
            MackieCommand.ChannelValues => new byte[0],
            MackieCommand.ChannelName => new byte[0],
            _ => null
        };
        if (packet.Command == (MackieCommand) 0x6)
        {
            Console.WriteLine($"Command 6 body: {BitConverter.ToString(packet.Body.ToArray())}");
        }
        if (responseBody is not null)
        {
            await client.SendResponse(packet, responseBody);
        }
    }
    
    if (packet.Type == MackiePacketType.Broadcast)
    {
        //var values = Enumerable.Range(2, packet.Body.Length / 4 - 2).Select(chunk => (chunk, packet.DecodeSingle(chunk)));
        //var max = values.Where(pair => pair.Item2 < 0).MaxBy(pair => pair.Item2);
        //Console.WriteLine($"Max: {max.Item1} : {max.Item2}");

        //Console.WriteLine(BitConverter.ToString(packet.Body.ToArray()));
    }
}

//byte[] ParseHex(string hex) => hex.Split(' ').Select(part => Convert.ToByte(part, 16)).ToArray();
*/