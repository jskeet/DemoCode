using DigiMixer.Mackie.Core;
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
    Console.WriteLine($"Got channel info: {packet.Body.Data.Length} bytes; {BitConverter.ToString(packet.Body.Data.Slice(0, 8).ToArray())}");
});
controller.MapCommandAction(MackieCommand.ChannelNames, packet =>
{
    Console.WriteLine("Received channel names");
    /*
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
    Console.WriteLine($"Names received: {count}");*/
});

controller.MapBroadcastAction(packet =>
{
    Console.WriteLine("Received broadcast");
});


var task = controller.Start();

await controller.SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty);
await controller.SendRequest(MackieCommand.ChannelInfoControl, new byte[8]);
await controller.SendRequest((MackieCommand) 3, MackiePacketBody.Empty);
// We don't use this info, but without sending it, we don't get any broadcasts.
await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 2 });
await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 3 });
await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 4 });
await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 0x12 });
//await controller.SendRequest(MackieCommand.ChannelInfoControl, new byte[] { 0, 0, 0, 6 });

await controller.SendRequest(MackieCommand.ChannelValues, new MackiePacketBodyBuilder(2).SetInt32(0, 1).SetInt32(1, 0x0500 | (100 << 16)).Build());

//var meterLayout = Enumerable.Range(1, 221).SelectMany(i => new byte[] { 0, 0, 0, (byte) i });
//await controller.SendRequest(MackieCommand.MeterLayout, new byte[] { 0, 0, 0, 1 }.Concat(meterLayout).ToArray());
//await controller.SendRequest(MackieCommand.BroadcastControl, new byte[] { 0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x01, 0x00, 0x00, 0x5a, 0x00, 0x01 });



for (int i = 0; i < 5; i++)
{
    // MasterFader sends a keep-alive every 2.5 seconds.
    await Task.Delay(2500);
    await controller.SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty);
}


