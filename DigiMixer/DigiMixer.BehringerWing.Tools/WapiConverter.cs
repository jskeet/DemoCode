using DigiMixer.Diagnostics;
using Newtonsoft.Json;
using System.Globalization;

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
            .Select(ParseSourceLine)
            .ToDictionary(node => node.name, node => node.hash);

        string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
        File.WriteAllText(JsonFile, json);
        Console.WriteLine($"Converted {nodes.Count} nodes.");
        return Task.FromResult(0);

        static (string name, uint hash) ParseSourceLine(string line)
        {
            // Sample line:
            // 		{     "cfg.mtr.$scopesrc",                     0xf1f55302,  I32, 0x0240, {    0} },

            var bits = line.Split(',');
            string name = bits[0].Split('"')[1];
            uint hash = uint.Parse(bits[1].Trim().Replace("0x", ""), NumberStyles.HexNumber);
            return new(name, hash);
        }
    }
}
