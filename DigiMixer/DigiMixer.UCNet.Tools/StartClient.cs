using DigiMixer.Diagnostics;
using DigiMixer.UCNet.Core;
using DigiMixer.UCNet.Core.Messages;
using Microsoft.Extensions.Logging;

public class StartClient : Tool
{
    public override async Task<int> Execute()
    {
        var factory = LoggerFactory.Create(builder => builder.AddConsole().AddSystemdConsole(options => { options.UseUtcTimestamp = true; options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'"; })
            .SetMinimumLevel(LogLevel.Trace));
        var logger = factory.CreateLogger("UCNetClient");

        var client = new UCNetClient(logger, "192.168.1.61", 53000);
        var listener = new UCNetMeterListener(logger);
        listener.Start();

        client.MessageReceived += (sender, message) => logger.LogInformation("Received {type} with mode {mode}", message.Type, ((uint) message.Mode).ToString("x8"));
        //client.MessageReceived += MaybeFormatJson;

        await client.Connect(default);
        client.Start();

        await client.SendAsync(new UdpMetersMessage(listener.LocalPort), default);
        var subscribe = JsonMessage.FromObject(new SubscribeBody(), MessageMode.ClientRequest);
        await client.SendAsync(subscribe, default);
        var presets = new FileRequestMessage("Listpresets/channel", 1000);
        await client.SendAsync(presets, default);

        while (true)
        {
            await Task.Delay(2000);
            await client.SendAsync(new KeepAliveMessage(), default);
        }
    }
}