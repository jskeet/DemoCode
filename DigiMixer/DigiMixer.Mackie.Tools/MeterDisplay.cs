using DigiMixer.Diagnostics;
using DigiMixer.Mackie.Core;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

namespace DigiMixer.Mackie.Tools;

public class MeterDisplay(string Address, string Port, string MeterCount) : Tool
{
    private static Meters? latestMeters;

    public override async Task<int> Execute()
    {
        var cts = new CancellationTokenSource();
        var task = Listen(Address, int.Parse(Port), int.Parse(MeterCount), cts.Token);
        Console.WriteLine("Press return to display the latest meters, or enter \"stop\" and press return to quit.");
        while (true)
        {
            string line = Console.ReadLine() ?? "";
            if (line == "stop")
            {
                break;
            }
            DisplayLatestMeters();
        }
        cts.Cancel();
        await task;
        return 0;
    }

    private static async Task Listen(string address, int port, int meterCount, CancellationToken token)
    {
        var controller = new MackieController(NullLogger.Instance, address, port);

        controller.MapCommand(MackieCommand.ClientHandshake, _ => new byte[] { 0x10, 0x40, 0xf0, 0x1d, 0xbc, 0xa2, 0x88, 0x1c });
        controller.MapCommand(MackieCommand.GeneralInfo, _ => new byte[] { 0, 0, 0, 2, 0, 0, 0x40, 0 });
        controller.MapCommand(MackieCommand.ChannelInfoControl, message => new MackieMessageBody(message.Body.Data.Slice(0, 4)));
        controller.MapBroadcastAction(HandleBroadcastMessage);
        await controller.Connect(default);
        controller.Start();

        // Send initial messages to say "we're here and want to be notified of changes"
        CancellationToken cancellationToken = default;
        await controller.SendRequest(MackieCommand.KeepAlive, MackieMessageBody.Empty, cancellationToken);
        await controller.SendRequest(MackieCommand.ClientHandshake, MackieMessageBody.Empty, cancellationToken);
        await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 2 }, cancellationToken);

        var layout = Enumerable.Range(1, meterCount).Select(BitConverter.GetBytes).SelectMany(array => array.Reverse()).ToArray();
        layout = new byte[] { 0, 0, 0, 1 }.Concat(layout).ToArray();
        await controller.SendRequest(MackieCommand.MeterLayout, layout, cancellationToken);
        await controller.SendRequest(MackieCommand.BroadcastControl,
            new byte[] { 0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x01, 0x00, 0x00, 0x5a, 0x00, 0x01 },
            cancellationToken);

        while (!token.IsCancellationRequested)
        {
            await Task.Delay(2000);
            await controller.SendRequest(MackieCommand.KeepAlive, MackieMessageBody.Empty, cancellationToken);
        }

        controller.Dispose();

        void HandleBroadcastMessage(MackieMessage message)
        {
            var body = message.Body;

            var values = new float[body.ChunkCount - 2];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = body.GetSingle(i + 2);
            }
            latestMeters = new Meters(body.GetInt32(0), body.GetInt32(1), values);
        }
    }

    private static void DisplayLatestMeters()
    {
        var meters = latestMeters;
        Console.WriteLine();
        if (meters is null)
        {
            Console.WriteLine("No meters");
            return;
        }
        Console.WriteLine($"Header 1: {meters.Header1}");
        Console.WriteLine($"Header 2: {meters.Header2}");
        var values = meters.Values;
        int start = 0;
        int cadence = 8;
        while (start < values.Length)
        {
            StringBuilder line = new StringBuilder();
            line.Append($"{start + 1,-5}");
            for (int i = 0; i < cadence && start + i < values.Length; i++)
            {
                line.Append($"{values[start + i],10:0.00}");
            }
            Console.WriteLine(line);
            start += cadence;
        }
    }

    private record Meters(int Header1, int Header2, float[] Values);
}
