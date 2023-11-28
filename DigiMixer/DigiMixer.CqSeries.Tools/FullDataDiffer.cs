using DigiMixer.Core;
using DigiMixer.CqSeries.Core;
using DigiMixer.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace DigiMixer.CqSeries.Tools;

/// <summary>
/// Sends "full data" requests once per second, and diffs the responses over time.
/// This should make it easy to interpret the "full data" response - at least for what we need.
/// </summary>
public class FullDataDiffer : Tool
{
    // Assume the address...
    private const string Address = "192.168.1.85";
    private const int Port = 51326;

    public override async Task<int> Execute()
    {
        var loggingFactory = LoggerFactory.Create(builder => builder.AddConsole().AddSimpleConsole(options => options.SingleLine = true)
            .SetMinimumLevel(LogLevel.Critical));

        var meterClient = new CqMeterClient(NullLogger.Instance /*loggingFactory.CreateLogger("Meter")*/);
        var controlClient = new CqControlClient(loggingFactory.CreateLogger("Control"), Address, Port);

        byte[]? currentSnapshot = null;

        IPEndPoint? mixerUdpEndpoint = null;
        controlClient.MessageReceived += (sender, message) =>
        {
            if (message is CqUdpHandshakeMessage handshake)
            {
                mixerUdpEndpoint = new IPEndPoint(IPAddress.Parse(Address), handshake.UdpPort);
            }
            else if (message is CqFullDataResponseMessage fullData)
            {
                var newSnapshot = fullData.Data.ToArray();
                DiffSnapshot(newSnapshot);
                currentSnapshot = newSnapshot;
            }
        };

        meterClient.Start();
        await controlClient.Connect(default);
        controlClient.Start();

        var handshake = new CqUdpHandshakeMessage(meterClient.LocalUdpPort);
        await controlClient.SendAsync(handshake, default);

        await Task.Delay(100);

        await controlClient.SendAsync(new CqVersionRequestMessage(), default);
        await Task.Delay(100);
        await controlClient.SendAsync(new CqUnknownMessage(new(CqMessageFormat.VariableLength, CqMessageType.ClientInitRequest, [0x02, 0x00])), default);

        while (true)
        {
            await Task.Delay(1000);
            if (mixerUdpEndpoint is IPEndPoint target)
            {
                await meterClient.SendKeepAliveAsync(target, default);
            }
            await controlClient.SendAsync(new CqFullDataRequestMessage(), default);
        }

        void DiffSnapshot(byte[] newSnapshot)
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
            void ReportDifference(string padding, int offset)
            {
                var length = Math.Min(currentSnapshot.Length - offset, 8);
                var currentData = currentSnapshot.AsSpan().Slice(offset, length);
                var newData = newSnapshot.AsSpan().Slice(offset, length);
                Console.WriteLine($"{padding}Difference at {offset:x4}: {Formatting.ToHex(currentData)} => {Formatting.ToHex(newData)}");
            }
        }
    }
}
