using DigiMixer.Diagnostics;
using DigiMixer.DmSeries.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.DmSeries.Tools;

public class WatchMixerMessages : Tool
{
    private const string Host = "192.168.1.86";
    private const int Port = 50368;

    public override async Task<int> Execute()
    {
        // Slightly ugly, but avoids the use of client being problematic in HandleMessage.
        DmClient client = null!;
        client = new DmClient(NullLogger.Instance, Host, Port, HandleMessage);
        await client.Connect(default);
        client.Start();

        // Request live updates for MMIX data.
        await Send(new DmMessage("MMIX", 0x01041000, []));

        while (true)
        {
            await Send(DmMessages.KeepAlive);
            await Task.Delay(1000);
        }

        Task HandleMessage(DmMessage message, CancellationToken cancellationToken)
        {
            // Don't log keepalive messages
            if (DmMessages.IsKeepAlive(message))
            {
                return Task.CompletedTask;
            }
            message.DisplayStructure("<=");
            return Task.CompletedTask;
        }

        async Task Send(DmMessage message)
        {
            if (!DmMessages.IsKeepAlive(message))
            {
                message.DisplayStructure(">=");
            }
            await client.SendAsync(message, default);
        }
    }
}
