using DigiMixer.Mackie.Core;
using System.Text;

namespace DigiMixer.MackieDump;

internal class Dumper
{
    internal static void Execute(string file)
    {
        using var input = File.OpenRead(file);
        var mc = MessageCollection.Parser.ParseFrom(input);

        // TODO: More interpretation
        foreach (var protoMessage in mc.Messages)
        {
            var timestamp = protoMessage.Timestamp.ToDateTimeOffset();
            var padding = protoMessage.Outbound ? "" : "    ";
            var message = new MackieMessage((byte) protoMessage.Sequence, (MackieMessageType) protoMessage.Type, (MackieCommand) protoMessage.Command, new MackieMessageBody(protoMessage.Data.Span));
            var body = message.Body;

            Console.Write($"{timestamp:HH:mm:ss.ffffff} {padding} {message.Sequence} {message.Type} {message.Command}");
            switch (message.Command)
            {
                case MackieCommand.ChannelValues when body.Length >= 8 && (body.GetInt32(1) & 0xff00) == 0x500:
                    {
                        int start = body.GetInt32(0);
                        Console.WriteLine();
                        Console.WriteLine($"  Chunk 1 (type): {body.GetInt32(1):x8}");
                        Console.WriteLine("  Values:");
                        for (int i = 0; i < body.ChunkCount - 2; i++)
                        {
                            int address = start + i;
                            var int32Value = body.GetInt32(i + 2);
                            var singleValue = body.GetSingle(i + 2);
                            Console.WriteLine($"    {address}: {int32Value} / {singleValue}");
                        }
                        break;
                    }
                case MackieCommand.ChannelNames when body.Length > 0:
                    {
                        int start = message.Body.GetInt32(0);
                        int count = (int) (body.GetUInt32(1) >> 16);
                        Console.WriteLine();
                        Console.WriteLine($"  Chunk 1 (type): {body.GetInt32(1):x8}");
                        Console.WriteLine("  Names:");
                        string allNames = Encoding.UTF8.GetString(message.Body.InSequentialOrder().Data.Slice(8).ToArray());
                        string[] names = allNames.Split('\0');
                        for (int i = 0; i < count; i++)
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
