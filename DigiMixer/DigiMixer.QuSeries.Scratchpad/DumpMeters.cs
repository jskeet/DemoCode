using DigiMixer.Diagnostics;
using DigiMixer.QuSeries.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.QuSeries.Scratchpad;

internal class DumpMeters
{
    static async void Main()
    {
        var logger = NullLogger.Instance;
        var api = new QuMixerApi(logger, "192.168.1.60", 51326);
        // This won't compile, as I removed MeterHandler after figuring out the meters.
        // It's easy to put back if we need this code again though.
        //api.MeterHandler += LogMeter;
        await api.Connect(default);
        await Task.Delay(1000);
        await api.SendKeepAlive();
        await Task.Delay(1000);
        api.Dispose();
    }

    static void LogMeter(QuGeneralPacket packet)
    {
        if (!packet.HasNonZeroData())
        {
            return;
        }
        Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff} Packet: Type={packet.Type}, Length={packet.Data.Length}");
        var lines = Hex.ConvertAndSplit(packet.Data, 20);
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
        Console.WriteLine();
    }
}
