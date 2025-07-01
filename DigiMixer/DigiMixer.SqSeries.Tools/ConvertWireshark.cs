using DigiMixer.Diagnostics;
using DigiMixer.AllenAndHeath.Core;
namespace DigiMixer.SqSeries.Tools;

public class ConvertWireshark(string file) : Tool
{
    public override async Task<int> Execute()
    {
        var dump = WiresharkDump.Load(file);
        var messages = await dump.ProcessMessages<AHRawMessage>("192.168.1.56", "192.168.1.140");

        var dir = Path.GetDirectoryName(file).OrThrow();
        var newName = Path.GetFileNameWithoutExtension(file) + " decoded.txt";
        using var writer = File.CreateText(Path.Combine(dir, newName));
        foreach (var message in messages)
        {
            message.DisplayStructure(writer);
        }
        Console.WriteLine($"Saved {newName}");
        return 0;
    }
}
