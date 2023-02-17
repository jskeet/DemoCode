using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace DigiMixer.QuSeries.Core;

public sealed class QuClient : IDisposable
{
    // TODO: Check this is correct for all clients.
    private static readonly byte[] KeepAlive = new byte[] { 0x7f, 0x25, 0, 0 };

    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;

    private TcpClient? tcpClient;
    private UdpClient? udpClient;
    private CancellationTokenSource? cts;
    private readonly SemaphoreSlim semaphore;

    public event EventHandler<QuControlPacket>? ControlPacketReceived;
    public event EventHandler<QuMeterPacket>? MeterPacketReceived;

    // The endpoint we send keep-alive packets to.
    private int? mixerUdpPort;

    public QuClient(ILogger logger, string host, int port)
    {
        this.logger = logger;
        this.host = host;
        this.port = port;
        semaphore = new SemaphoreSlim(1);
    }

    public async Task SendKeepAliveAsync(CancellationToken cancellationToken)
    {
        if (udpClient is null || mixerUdpPort is not int port)
        {
            throw new InvalidOperationException("Client isn't running.");
        }
        await udpClient.SendAsync(KeepAlive, host, port, cancellationToken);
    }

    public async Task SendAsync(QuControlPacket packet, CancellationToken cancellationToken)
    {
        if (tcpClient is null)
        {
            throw new InvalidOperationException("Client isn't running.");
        }

        var data = packet.ToByteArray();
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Sending packet: {packet}", packet);
        }
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await tcpClient!.GetStream().WriteAsync(data, 0, data.Length, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task Start()
    {
        cts = new CancellationTokenSource();
        tcpClient = new TcpClient { NoDelay = true };
        int localUdpPort = FindAvailableUdpPort();
        udpClient = new UdpClient(localUdpPort);

        var udpTask = StartUdp(udpClient);

        var packetBuffer = new QuPacketBuffer();

        try
        {
            // TODO: Use ConnectAsync instead? We don't really want to hand control back to the caller until this has completed though...
            tcpClient.Connect(host, port);

            // Send the first handshake packet
            await SendAsync(QuControlPacket.Create(type: 0, new byte[] { (byte) (localUdpPort & 0xff), (byte) (localUdpPort >> 8) }), cts?.Token ?? new CancellationToken(true));

            var stream = tcpClient.GetStream();
            byte[] buffer = new byte[32768];
            while (cts?.IsCancellationRequested == false)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cts?.Token ?? new CancellationToken(true));
                packetBuffer.Process(buffer.AsSpan().Slice(0, bytesRead), ProcessPacket);
            }
        }
        catch (Exception e) when (cts.IsCancellationRequested)
        {
            // TODO: Work out if this is actually useful.
            logger.LogInformation("Operation aborted due to controller shutdown", e);
        }
        finally
        {
            Dispose();
        }

        await udpTask;
    }

    private async Task StartUdp(UdpClient client)
    {
        while (cts?.IsCancellationRequested == false)
        {
            var result = await client.ReceiveAsync(cts?.Token ?? new CancellationToken(true));
            var packet = new QuMeterPacket(result.Buffer);
            MeterPacketReceived?.Invoke(this, packet);
        }
    }

    private void ProcessPacket(QuControlPacket packet)
    {
        // Initial handshake response.
        if (packet is QuGeneralPacket { Type: 0 } response && response.Data.Length == 2)
        {
            mixerUdpPort = response.Data[0] + (response.Data[1] << 8);
            logger.LogInformation("Received mixer UDP port: {port}", mixerUdpPort);
        }
        else
        {
            ControlPacketReceived?.Invoke(this, packet);
        }
    }

    private static int FindAvailableUdpPort()
    {
        // TODO: Work out why we have to do this and then create a new client, rather than just using
        // the client we've created here.
        using var temporaryClient = new UdpClient();
        temporaryClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
        return ((IPEndPoint) temporaryClient.Client.LocalEndPoint!).Port;
    }

    public void Dispose()
    {
        cts?.Cancel();
        tcpClient?.Dispose();
        udpClient?.Dispose();
        cts = null;
        tcpClient = null;
        udpClient = null;
    }
}
