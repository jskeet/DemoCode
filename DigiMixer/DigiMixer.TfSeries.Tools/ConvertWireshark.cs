using DigiMixer.Diagnostics;
using DigiMixer.Yamaha;
using DigiMixer.Yamaha.Core;

namespace DigiMixer.TfSeries.Tools;

public class ConvertWireshark(string file) : Tool
{
    public override async Task<int> Execute()
    {
        var dump = WiresharkDump.Load(file);
        var messages = await dump.ProcessMessages<YamahaMessage>("192.168.1.96", "192.168.1.140");

        var dir = Path.GetDirectoryName(file).OrThrow();
        var newName = Path.GetFileNameWithoutExtension(file) + " decoded.txt";
        using var writer = File.CreateText(Path.Combine(dir, newName));
        var schemaDictionary = new Dictionary<string, SchemaCol>(StringComparer.Ordinal);
        foreach (var message in messages)
        {
            message.DisplayStructure(DecodingOptions.Investigative, writer, schemaDictionary.GetValueOrDefault);
            var schemaMessage = SectionSchemaAndDataMessage.TryParse(message.Message);
            if (schemaMessage is not null)
            {
                schemaDictionary[schemaMessage.Subtype] = schemaMessage.Data.Schema;
            }
        }
        Console.WriteLine($"Saved {newName}");
        return 0;
    }
}
