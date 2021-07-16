using Commons.Music.Midi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MidiExplorer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var deviceId = args[0];

            var input = await MidiAccessManager.Default.OpenInputAsync(MidiAccessManager.Default.Inputs.Single(port => port.Name == deviceId).Id);
            var output = await MidiAccessManager.Default.OpenOutputAsync(MidiAccessManager.Default.Outputs.Single(port => port.Name == deviceId).Id);

            input.MessageReceived += (sender, args) =>
            {
                var data = args.Data.Skip(args.Start).Take(args.Length).ToArray();
                Console.WriteLine($"Received: {BitConverter.ToString(data).Replace("-", " ")}");
            };

            while (true)
            {
                Console.Write("Data: ");
                var line = Console.ReadLine();
                var data = ParseLine(line);
                output.Send(data, 0, data.Length, 0);
            }
        }

        private static byte[] ParseLine(string line)
        {
            line = line.Replace(" ", "");
            byte[] data = new byte[line.Length / 2];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)((ParseNybble(line[i * 2]) << 4) + ParseNybble(line[i * 2 + 1]));
            }
            return data;

            int ParseNybble(char c) => "0123456789abcdef".IndexOf(char.ToLowerInvariant(c));
        }
    }
}
