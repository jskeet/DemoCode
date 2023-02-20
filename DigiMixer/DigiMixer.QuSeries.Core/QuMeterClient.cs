using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace DigiMixer.QuSeries.Core;

public class QuMeterClient : IDisposable
{
    // TODO: Check this is correct for all clients.
    private static readonly byte[] KeepAlive = new byte[] { 0x7f, 0x25, 0, 0 };

    private readonly ILogger logger;
    private readonly string host;
    private CancellationTokenSource cts;
    private UdpClient? udpClient;

    public int LocalUdpPort { get; }
    public event EventHandler<QuGeneralPacket>? PacketReceived;

    public QuMeterClient(ILogger logger, string host)
    {
        this.logger = logger;
        this.host = host;
        cts = new CancellationTokenSource();
        LocalUdpPort = FindAvailableUdpPort();
    }

    public async Task SendKeepAliveAsync(int mixerUdpPort, CancellationToken cancellationToken)
    {
        if (udpClient is null)
        {
            throw new InvalidOperationException("Client isn't running.");
        }
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Sending keep-alive packet");
        }
        await udpClient.SendAsync(KeepAlive, host, mixerUdpPort, cancellationToken);
    }

    public async Task Start()
    {
        udpClient = new UdpClient(LocalUdpPort);

        while (!cts.IsCancellationRequested)
        {
            var result = await udpClient.ReceiveAsync(cts.Token);
            if (QuControlPacket.TryParse(result.Buffer) is QuGeneralPacket packet)
            {
                if (logger.IsEnabled(LogLevel.Trace) && packet.HasNonZeroData())
                {
                    logger.LogTrace("Received packet: {packet}", packet);
                }
                PacketReceived?.Invoke(this, packet);
            }
        }
    }

    private static int FindAvailableUdpPort()
    {
        // TODO: Move this somewhere common. Using port 0 doesn't prompt Windows for
        // appropriate rights, which are needed to actually receive packets - we
        // need a specific port for that.
        using var temporaryClient = new UdpClient();
        temporaryClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
        return ((IPEndPoint) temporaryClient.Client.LocalEndPoint!).Port;
    }

    public void Dispose()
    {
        cts.Cancel();
        udpClient?.Dispose();
        udpClient = null;
    }
}
