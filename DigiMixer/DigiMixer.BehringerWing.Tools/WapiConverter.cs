using DigiMixer.Diagnostics;
using Newtonsoft.Json;

namespace DigiMixer.BehringerWing.Tools;

public class WapiConverter(string SourceFile, string JsonFile) : Tool
{
    public override Task<int> Execute()
    {
        var sourceLines = File.ReadAllLines(SourceFile);

        var nodes = sourceLines
            .SkipWhile(line => !line.Contains("wingdataset"))
            .Skip(1)
            .TakeWhile(line => line.Contains('{'))
            .Select(NodeDescription.FromSourceLine)
            .ToList();

        string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
        File.WriteAllText(JsonFile, json);
        Console.WriteLine($"Converted {nodes.Count} nodes.");
        return Task.FromResult(0);
    }
}
