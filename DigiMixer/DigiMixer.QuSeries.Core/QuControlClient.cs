using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace DigiMixer.QuSeries.Core;

public sealed class QuControlClient : IDisposable
{
    private readonly ILogger logger;
    private readonly string host;
    private readonly int port;

    private TcpClient? tcpClient;
    private CancellationTokenSource? cts;
    private readonly SemaphoreSlim semaphore;

    public event EventHandler<QuControlPacket>? PacketReceived;

    public QuControlClient(ILogger logger, string host, int port)
    {
        this.logger = logger;
        this.host = host;
        this.port = port;
        semaphore = new SemaphoreSlim(1);
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
        var packetBuffer = new QuPacketBuffer();
        try
        {
            // TODO: Use ConnectAsync instead? We don't really want to hand control back to the caller until this has completed though...
            tcpClient.Connect(host, port);

            var stream = tcpClient.GetStream();
            byte[] buffer = new byte[32768];
            while (cts?.IsCancellationRequested == false)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cts?.Token ?? new CancellationToken(true));
                if (bytesRead == 0)
                {
                    throw new InvalidOperationException("Unexpected TCP stream termination");
                }
                packetBuffer.Process(buffer.AsSpan().Slice(0, bytesRead), ProcessPacket);
            }
        }
        catch (Exception e) when (cts?.IsCancellationRequested != false)
        {
            // TODO: Work out if this is actually useful.
            logger.LogInformation("Operation aborted due to controller shutdown", e);
        }
        finally
        {
            Dispose();
        }
    }

    private void ProcessPacket(QuControlPacket packet)
    {
        logger.LogTrace("Received packet: {packet}", packet);
        PacketReceived?.Invoke(this, packet);
    }

    public void Dispose()
    {
        cts?.Cancel();
        tcpClient?.Dispose();
        cts = null;
        tcpClient = null;
    }
}
