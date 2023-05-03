using DigiMixer.Mackie.Core;
using DigiMixer.MackieDump;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

if (args.Length == 0)
{
    Console.WriteLine("Required command line arguments: <mode> <options>");
    Console.WriteLine("Modes: record or dump");
    Console.WriteLine("Record options: <IP address> <port - typically 50001> <output file>");
    Console.WriteLine("Dump options: <file>");
    return;
}

if (args[0] == "record")
{
    if (args.Length != 4)
    {
        Console.WriteLine("Record options: <IP address> <port - typically 50001> <output file>");
        return;
    }
    string address = args[1];
    int port = int.Parse(args[2]);
    string file = args[3];
    await Record(address, port, file);
}
else if (args[0] == "dump")
{
    if (args.Length != 2)
    {
        Console.WriteLine("Dump options: <file>");
        return;
    }
    Dump(args[1]);
}
else
{
    Console.WriteLine($"Unknown mode: {args[0]}");
    return;
}

async Task Record(string address, int port, string file)
{
    PacketCollection pc = new PacketCollection();
    var controller = new MackieController(NullLogger.Instance, address, port);
    controller.PacketSent += (sender, packet) => RecordPacket(packet, true);
    controller.PacketReceived += (sender, packet) => RecordPacket(packet, false);

    controller.MapCommand(MackieCommand.ClientHandshake, _ => new byte[] { 0x10, 0x40, 0xf0, 0x1d, 0xbc, 0xa2, 0x88, 0x1c });
    controller.MapCommand(MackieCommand.GeneralInfo, _ => new byte[] { 0, 0, 0, 2, 0, 0, 0x40, 0 });
    controller.MapCommand(MackieCommand.ChannelInfoControl, packet => new MackiePacketBody(packet.Body.Data.Slice(0, 4)));
    await controller.Connect(default);
    controller.Start();

    // From MackieMixerApi.Connect
    CancellationToken cancellationToken = default;
    await controller.SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty, cancellationToken);
    await controller.SendRequest(MackieCommand.ChannelInfoControl, new byte[8], cancellationToken);
    await controller.SendRequest((MackieCommand) 3, MackiePacketBody.Empty, cancellationToken);
    await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 2 }, cancellationToken);

    // From MackieMixerApi.RequestChannelData
    await controller.SendRequest(MackieCommand.ChannelInfoControl, new MackiePacketBody(new byte[] { 0, 0, 0, 6 }), cancellationToken);
    // Give some time to receive all the channel data
    await Task.Delay(2000);
    await controller.SendRequest(MackieCommand.KeepAlive, MackiePacketBody.Empty, cancellationToken);

    // From MackieMixerApi.RequestAllData
    var versionInfo = await controller.SendRequest(MackieCommand.FirmwareInfo, MackiePacketBody.Empty);
    var modelInfo = await controller.SendRequest(MackieCommand.GeneralInfo, new MackiePacketBody(new byte[] { 0, 0, 0, 0x12 }));
    var generalInfo = await controller.SendRequest(MackieCommand.GeneralInfo, new MackiePacketBody(new byte[] { 0, 0, 0, 3 }));

    // Give some time to receive the remaining data
    await Task.Delay(2000);

    controller.Dispose();

    Console.WriteLine($"Captured {pc.Packets.Count} packets");

    using var output = File.Create(file);
    pc.WriteTo(output);

    void RecordPacket(MackiePacket packet, bool outbound)
    {
        pc.Packets.Add(ConvertPacket(packet, outbound));
        // Immediate uninterpreted display, truncated after 16 bytes of data.
        var padding = outbound ? "" : "    ";
        if (packet.Body.Data.Length == 0)
        {
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.ffffff} {padding} {packet.Sequence} {packet.Type} {packet.Command} (empty)");
        }
        else
        {
            var dataLength = $"({packet.Body.Data.Length} bytes)";
            var data = BitConverter.ToString(packet.Body.Data.ToArray()).Replace("-", " ");
            if (data.Length > 47)
            {
                data = data.Substring(0, 47) + "...";
            }
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.ffffff} {padding} {packet.Sequence} {packet.Type} {packet.Command}: {dataLength}: {data}");
        }

        Packet ConvertPacket(MackiePacket packet, bool outbound) =>
            new Packet
            {
                Outbound = outbound,
                Command = (int) packet.Command,
                Type = (int) packet.Type,
                Data = ByteString.CopyFrom(packet.Body.Data),
                Sequence = packet.Sequence,
                Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow)
            };
    }
}

void Dump(string file)
{
    using var input = File.OpenRead(file);
    var pc = PacketCollection.Parser.ParseFrom(input);

    // TODO: More interpretation
    foreach (var protoPacket in pc.Packets)
    {
        var timestamp = protoPacket.Timestamp.ToDateTimeOffset();
        var padding = protoPacket.Outbound ? "" : "    ";
        var packet = new MackiePacket((byte) protoPacket.Sequence, (MackiePacketType) protoPacket.Type, (MackieCommand) protoPacket.Command, new MackiePacketBody(protoPacket.Data.Span));
        var body = packet.Body;

        Console.Write($"{timestamp:HH:mm:ss.ffffff} {padding} {packet.Sequence} {packet.Type} {packet.Command}");
        switch (packet.Command)
        {
            case MackieCommand.ChannelValues when body.Length >= 8 && (body.GetInt32(1) & 0xff00) == 0x500:
                {
                    int start = body.GetInt32(0);
                    Console.WriteLine();
                    Console.WriteLine("  Values:");
                    for (int i = 2; i < body.ChunkCount; i++)
                    {
                        int address = start - 2 + i;
                        var int32Value = body.GetInt32(i);
                        var singleValue = body.GetSingle(i);
                        Console.WriteLine($"    {address}: {int32Value} / {singleValue}");
                    }
                    break;
                }
            case MackieCommand.ChannelNames when body.Length > 0:
                {
                    int start = packet.Body.GetInt32(0);
                    Console.WriteLine();
                    Console.WriteLine("  Names:");
                    string allNames = Encoding.ASCII.GetString(packet.Body.InSequentialOrder().Data.Slice(8).ToArray());
                    string[] names = allNames.Split('\0');
                    for (int i = 0; i < names.Length; i++)
                    {
                        int address = start + i;
                        Console.WriteLine($"    {address}: {names[i]}");
                    }
                    break;
                }
            default:
                var data = BitConverter.ToString(body.Data.ToArray()).Replace("-", " ");
                Console.WriteLine($": {data}");
                break;
        }
    }
}
