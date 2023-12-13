using DigiMixer.Core;
using DigiMixer.Diagnostics;
using DigiMixer.DmSeries.Core;

namespace DigiMixer.DmSeries.Tools;

public class ProcessTextDump(string FileName, string Mode) : Tool
{
    public override Task<int> Execute()
    {
        Action<DmMessage, string> processingAction = Mode switch
        {
            "full" => DmMessageExtensions.DisplayStructure,
            "brief" => DmMessageExtensions.DisplaySummary,
            _ => throw new ArgumentException($"Invalid mode: '{Mode}'; must be 'full' or 'brief'")
        };

        var inboundProcessor = new MessageProcessor<DmMessage>(DmMessage.TryParse, m => m.Length, m => processingAction(m, "<="));
        var outboundProcessor = new MessageProcessor<DmMessage>(DmMessage.TryParse, m => m.Length, m => processingAction(m, "=>"));

        using var reader = File.OpenText(FileName);
        while (reader.ReadLine() is string line)
        {
            var hexDumpLine = Hex.ParseHexDumpLine(line);
            var processor = hexDumpLine.Direction == Hex.Direction.Outbound ? outboundProcessor : inboundProcessor;
            processor.Process(hexDumpLine.Data);
        }
        Console.WriteLine($"Processed outbound messages: {outboundProcessor.MessagesProcessed}");
        Console.WriteLine($"Processed inbound messages: {inboundProcessor.MessagesProcessed}");

        Console.WriteLine($"Unprocessed outbound data: {outboundProcessor.UnprocessedLength}");
        Console.WriteLine($"Unprocessed inbound data: {inboundProcessor.UnprocessedLength}");

        return Task.FromResult(0);
    }
}
