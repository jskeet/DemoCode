using DigiMixer.UCNet.Core.Messages;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace DigiMixer.UCNet.Core;

public class UCNetMeterListener : IDisposable
{
    public int Port => ((IPEndPoint) client.Client.LocalEndPoint!).Port;

    private readonly ILogger logger;
    private readonly UdpClient client;
    private readonly CancellationTokenSource cts;

    public event EventHandler<Meter16Message>? MessageReceived;

    public UCNetMeterListener(ILogger logger, int port)
    {
        client = new UdpClient(new IPEndPoint(IPAddress.Any, port));
        cts = new CancellationTokenSource();
        this.logger = logger;
        Console.WriteLine(client.Client.LocalEndPoint);
    }

    public UCNetMeterListener(ILogger logger) : this(logger, FindAvailablePort())
    {
    }

    // For some reason, using port 0 doesn't prompt in Windows when it needs to get permission.
    // It's find for just getting an available port though.
    private static int FindAvailablePort()
    {
        using (var tmpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0)))
        {
            var endpoint = (IPEndPoint) tmpClient.Client.LocalEndPoint!;
            return endpoint.Port;
        }
    }

    public async Task Start()
    {
        try
        {
            while (!cts.IsCancellationRequested)
            {
                var result = await client.ReceiveAsync(cts.Token);
                var message = UCNetMessage.TryParseUdp(result.Buffer);
                switch (message)
                {
                    case null:
                        logger.LogWarning("Received UDP packet length {length} which isn't a full message", result.Buffer.Length);
                        break;
                    case Meter16Message meters:
                        MessageReceived?.Invoke(this, meters);
                        break;
                    default:
                        logger.LogWarning("Received UDP packet with unexpected meter type {type}", message.Type);
                        break;
                }
                var buffer = result.Buffer;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in receive loop");
            throw;
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        client.Dispose();
    }
}
