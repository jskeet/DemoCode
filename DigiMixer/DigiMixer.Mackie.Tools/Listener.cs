using DigiMixer.Mackie.Core;
using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.Mackie.Tools;

/// <summary>
/// Connects to the mixer but sends minimal messages - just keep-alives - listening
/// (and saving) incoming messages until the user hits return.
/// </summary>
internal class Listener
{
    internal static async Task ExecuteAsync(string address, int port, string file)
    {
        var cts = new CancellationTokenSource();
        var task = Listen(address, port, cts.Token);
        Console.WriteLine("Press return to stop listening");
        Console.ReadLine();
        cts.Cancel();
        var pc = await task;
        using var output = File.Create(file);
        pc.WriteTo(output);
    }

    private static async Task<MessageCollection> Listen(string address, int port, CancellationToken token)
    {
        MessageCollection mc = new MessageCollection();
        var controller = new MackieController(NullLogger.Instance, address, port);
        controller.MessageSent += (sender, message) => RecordMessage(message, true);
        controller.MessageReceived += (sender, message) => RecordMessage(message, false);

        controller.MapCommand(MackieCommand.ClientHandshake, _ => new byte[] { 0x10, 0x40, 0xf0, 0x1d, 0xbc, 0xa2, 0x88, 0x1c });
        controller.MapCommand(MackieCommand.GeneralInfo, _ => new byte[] { 0, 0, 0, 2, 0, 0, 0x40, 0 });
        controller.MapCommand(MackieCommand.ChannelInfoControl, message => new MackieMessageBody(message.Body.Data.Slice(0, 4)));
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

        Console.WriteLine($"Captured {mc.Messages.Count} messages");
        return mc;

        void RecordMessage(MackieMessage message, bool outbound)
        {
            mc.Messages.Add(Message.FromMackieMessage(message, outbound, null));
            // Immediate uninterpreted display, truncated after 16 bytes of data.
            var padding = outbound ? "" : "    ";
            if (message.Body.Data.Length == 0)
            {
                Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.ffffff} {padding} {message.Sequence} {message.Type} {message.Command} (empty)");
            }
            else
            {
                var dataLength = $"({message.Body.Data.Length} bytes)";
                var data = BitConverter.ToString(message.Body.Data.ToArray()).Replace("-", " ");
                if (data.Length > 47)
                {
                    data = data.Substring(0, 47) + "...";
                }
                Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.ffffff} {padding} {message.Sequence} {message.Type} {message.Command}: {dataLength}: {data}");
            }
        }
    }
}
