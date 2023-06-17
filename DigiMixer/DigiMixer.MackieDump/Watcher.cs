using DigiMixer.Mackie.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.MackieDump;
internal class Watcher
{
    internal static async Task ExecuteAsync(string address, int port)
    {
        var cts = new CancellationTokenSource();
        var task = Listen(address, port, cts.Token);
        Console.WriteLine("Press return to stop listening");
        Console.ReadLine();
        cts.Cancel();
        await task;
    }

    private static async Task Listen(string address, int port, CancellationToken token)
    {
        var controller = new MackieController(NullLogger.Instance, address, port);

        controller.MapCommand(MackieCommand.ClientHandshake, _ => new byte[] { 0x10, 0x40, 0xf0, 0x1d, 0xbc, 0xa2, 0x88, 0x1c });
        controller.MapCommand(MackieCommand.GeneralInfo, _ => new byte[] { 0, 0, 0, 2, 0, 0, 0x40, 0 });
        controller.MapCommand(MackieCommand.ChannelInfoControl, message => new MackieMessageBody(message.Body.Data.Slice(0, 4)));
        controller.MapCommand(MackieCommand.ChannelInfoControl, message => new MackieMessageBody(message.Body.Data.Slice(0, 4)));
        controller.MapCommandAction(MackieCommand.ChannelValues, HandleChannelValues);
        await controller.Connect(default);
        controller.Start();

        // Send initial messages to say "we're here and want to be notified of changes"
        CancellationToken cancellationToken = default;
        await controller.SendRequest(MackieCommand.KeepAlive, MackieMessageBody.Empty, cancellationToken);
        await controller.SendRequest(MackieCommand.ClientHandshake, MackieMessageBody.Empty, cancellationToken);
        await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 2 }, cancellationToken);

        while (!token.IsCancellationRequested)
        {
            await Task.Delay(2000);
            await controller.SendRequest(MackieCommand.KeepAlive, MackieMessageBody.Empty, cancellationToken);
        }

        controller.Dispose();

        void HandleChannelValues(MackieMessage message)
        {
            var body = message.Body;
            if (body.Length < 8)
            {
                return;
            }
            uint chunk1 = body.GetUInt32(1);
            // TODO: Handle other value types.
            if ((chunk1 & 0xff00) != 0x0500)
            {
                return;
            }
            // Don't display huge "all values" messages.
            if (body.ChunkCount > 5)
            {
                return;
            }
            int start = body.GetInt32(0);
            for (int i = 0; i < body.ChunkCount - 2; i++)
            {
                int address = start + i;
                int int32Value = body.GetInt32(i + 2);
                float singleValue = body.GetSingle(i + 2);
                Console.WriteLine($"{address}: {int32Value} / {singleValue}");
            }
        }
    }
}
