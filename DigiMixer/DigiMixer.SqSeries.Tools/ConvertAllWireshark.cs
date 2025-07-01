using DigiMixer.Diagnostics;

namespace DigiMixer.SqSeries.Tools;

public class ConvertAllWireshark(string directory) : Tool
{
    public override async Task<int> Execute()
    {
        foreach (var file in Directory.GetFiles(directory, "*.pcapng"))
        {
            var singleFileTool = new ConvertWireshark(file);
            var result = await singleFileTool.Execute();
            if (result != 0)
            {
                return result;
            }
        }
        return 0;
    }
}
