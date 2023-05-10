using DigiMixer.Mackie.Core;
using System.Text;

namespace DigiMixer.MackieDump;

internal class Dumper
{
    internal static void Execute(string file)
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
}
