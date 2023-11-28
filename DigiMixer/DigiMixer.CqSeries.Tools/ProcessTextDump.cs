using DigiMixer.Core;
using DigiMixer.CqSeries.Core;
using DigiMixer.Diagnostics;

namespace DigiMixer.CqSeries.Tools;

public class ProcessTextDump(string FileName) : Tool
{
    public override Task<int> Execute()
    {
        var inboundProcessor = new MessageProcessor<CqRawMessage>(CqRawMessage.TryParse, m => m.Length, m => Display("<=", m));
        var outboundProcessor = new MessageProcessor<CqRawMessage>(CqRawMessage.TryParse, m => m.Length, m => Display("=>", m));

        using var reader = File.OpenText(FileName);
        while (reader.ReadLine() is string line)
        {
            var hexDumpLine = Hex.ParseHexDumpLine(line);
            var processor = hexDumpLine.Direction == Hex.Direction.Outbound ? outboundProcessor : inboundProcessor;
            processor.Process(hexDumpLine.Data);
        }

        Console.WriteLine($"Unprocessed outbound data: {outboundProcessor.UnprocessedLength}");
        Console.WriteLine($"Unprocessed inbound data: {inboundProcessor.UnprocessedLength}");

        return Task.FromResult(0);
    }

    private static void Display(string direction, CqRawMessage message)
    {
        Console.WriteLine($"{direction}: {message}");
    }
}
