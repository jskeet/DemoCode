using DigiMixer.Core;
using DigiMixer.Diagnostics;
using DigiMixer.DmSeries.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.DmSeries.Tools;

public class MmixFullDataDiff : Tool
{
    public override async Task<int> Execute()
    {
        var mixer = new DmMixerApi(NullLogger.Instance, "192.168.1.86", 50368);
        await mixer.Connect(default);

        byte[]? currentSnapshot = null;
        while (true)
        {
            var channelData = await mixer.RequestFullData(DmMessages.Types.Channels, DmMessages.Subtypes.Channels, default);
            var newSnapshot = ((DmBinarySegment) channelData.Segments[7]).Data.ToArray();
            DiffSnapshot(currentSnapshot, newSnapshot);
            currentSnapshot = newSnapshot;

            await Task.Delay(mixer.KeepAliveInterval);
            await mixer.SendKeepAlive();
        }
    }

    private static void DiffSnapshot(byte[]? currentSnapshot, byte[] newSnapshot)
    {
        if (currentSnapshot is null)
        {
            return;
        }

        if (currentSnapshot.Length != newSnapshot.Length)
        {
            Console.WriteLine($"Lengths differ: {currentSnapshot.Length} != {newSnapshot.Length}");
            return;
        }

        List<int> differences = new();
        for (int i = 0; i < currentSnapshot.Length; i++)
        {
            if (newSnapshot[i] != currentSnapshot[i])
            {
                differences.Add(i);
                // We'll report 8 bytes of differences, so there's no point in reporting each byte separately.
                i += 8;
                continue;
            }
        }
        switch (differences.Count)
        {
            case 0:
                return;
            case 1:
                ReportDifference("", differences[0]);
                break;
            case int x when x <= 10:
                Console.WriteLine($"{x} differences:");
                foreach (var offset in differences)
                {
                    ReportDifference("  ", offset);
                }
                break;
            case int x:
                Console.WriteLine($"{x} differences - too many to report.");
                break;
        }
        Console.WriteLine();

        void ReportDifference(string padding, int offset)
        {
            var length = Math.Min(currentSnapshot.Length - offset, 8);
            var currentData = currentSnapshot.AsSpan().Slice(offset, length);
            var newData = newSnapshot.AsSpan().Slice(offset, length);
            Console.WriteLine($"{padding}Difference at 0x{offset:x4}: {Formatting.ToHex(currentData)} => {Formatting.ToHex(newData)}");
        }
    }
}
