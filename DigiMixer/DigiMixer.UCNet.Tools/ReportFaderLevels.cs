using DigiMixer.Diagnostics;
using DigiMixer.UCNet.Core;
using DigiMixer.UCNet.Core.Messages;
using Microsoft.Extensions.Logging;

/// <summary>
/// Tool to report fader levels as reported by the meter messages.
/// Note that this appears to stop reporting values after a little while;
/// it's not clear why. The real client doesn't seem to have that problem.
/// </summary>
public class ReportFaderLevels : Tool
{
    public override async Task<int> Execute()
    {
        var factory = LoggerFactory.Create(builder => builder.AddConsole().AddSystemdConsole(options => { options.UseUtcTimestamp = true; options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'"; })
            .SetMinimumLevel(LogLevel.Trace));
        var logger = factory.CreateLogger("UCNetClient");

        var client = new UCNetClient(logger, "192.168.1.61", 53000);
        var listener = new UCNetMeterListener(logger);
        listener.MessageReceived += ReportMainFaderLevel;
        listener.Start();

        client.MessageReceived += ReportMainFaderLevel;
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

        void ReportMainFaderLevel(object? sender, UCNetMessage message)
        {
            if (message is not Meter16Message { MeterType: "fdrs" } meterMessage)
            {
                return;
            }
            foreach (var row in meterMessage.Rows)
            {
                if (row.Source == MeterSource.Main)
                {
                    var rawValue = row.GetValue(0);
                    Console.WriteLine($"Main level: {rawValue}");
                }
            }
        }
    }
}