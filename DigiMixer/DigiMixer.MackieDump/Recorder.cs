using DigiMixer.Mackie.Core;
using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.MackieDump;

internal class Recorder
{
    internal static async Task ExecuteAsync(string address, int port, string file)
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

        // From MackieMixerApi.Connect
        CancellationToken cancellationToken = default;
        await controller.SendRequest(MackieCommand.KeepAlive, MackieMessageBody.Empty, cancellationToken);
        await controller.SendRequest(MackieCommand.ChannelInfoControl, new byte[8], cancellationToken);
        await controller.SendRequest(MackieCommand.ClientHandshake, MackieMessageBody.Empty, cancellationToken);
        await controller.SendRequest(MackieCommand.GeneralInfo, new byte[] { 0, 0, 0, 2 }, cancellationToken);

        // From MackieMixerApi.RequestChannelData
        await controller.SendRequest(MackieCommand.ChannelInfoControl, new MackieMessageBody(new byte[] { 0, 0, 0, 6 }), cancellationToken);
        // Give some time to receive all the channel data
        await Task.Delay(2000);
        await controller.SendRequest(MackieCommand.KeepAlive, MackieMessageBody.Empty, cancellationToken);

        // From MackieMixerApi.RequestAllData
        var versionInfo = await controller.SendRequest(MackieCommand.FirmwareInfo, MackieMessageBody.Empty);
        // Sending a DL16S model-info request to a DL32R crashes it.
        // var modelInfo = await controller.SendRequest(MackieCommand.GeneralInfo, new MackieMessageBody(new byte[] { 0, 0, 0, 0x12 }));
        var generalInfo = await controller.SendRequest(MackieCommand.GeneralInfo, new MackieMessageBody(new byte[] { 0, 0, 0, 3 }));

        // Give some time to receive the remaining data
        await Task.Delay(2000);

        controller.Dispose();

        Console.WriteLine($"Captured {mc.Messages.Count} messages");

        using var output = File.Create(file);
        mc.WriteTo(output);

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
