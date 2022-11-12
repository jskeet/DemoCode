using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace DigiMixer.Mackie;

public class MackieClient : IDisposable
{
    private static readonly byte[] emptyBody = new byte[0];

    private readonly ILogger logger;
    private readonly CancellationTokenSource cts;
    private TcpClient tcpClient;
    private Stream stream;
    private byte nextSequenceNumber;

    public EventHandler<MackiePacket>? PacketReceived;

    public MackieClient(ILogger logger, string host, int port)
    {
        tcpClient = new TcpClient(host, port) { NoDelay = true };
        stream = tcpClient.GetStream();
        this.logger = logger;
        cts = new CancellationTokenSource();
    }

    public async Task Send(MackiePacket packet)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Sending packet: {packet}", packet);
        }
        await stream.WriteAsync(packet.ToByteArray());
    }

    public async Task<MackiePacket> SendRequest(MackieCommand type, MackiePacketBody body)
    {
        byte seq = ++nextSequenceNumber;
        var request = new MackiePacket(seq, MackiePacketType.Request, type, body);
        await Send(request);
        return request;
    }

    public async Task SendResponse(MackiePacket request, MackiePacketBody body)
    {
        var response = new MackiePacket(request.Sequence, MackiePacketType.Response, request.Command, body);
        await Send(response);
    }

    public async Task StartReceiving()
    {
        // TODO: Is this enough? In theory I guess it could be 256K...
        byte[] buffer = new byte[65536];
        int position = 0;

        try
        {
            while (!cts.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(buffer, position, buffer.Length - position);
                position += bytesRead;
                while (position != 0 && MackiePacket.TryParse(buffer, 0, position) is MackiePacket packet)
                {
                    if (logger.IsEnabled(LogLevel.Trace))
                    {
                        logger.LogTrace("Received packet: {packet}", packet);
                    }
                    PacketReceived?.Invoke(this, packet);
                    if (position == packet.Length)
                    {
                        position = 0;
                    }
                    else
                    {
                        // TODO: We could be smarter about this to avoid copying...
                        Buffer.BlockCopy(buffer, packet.Length, buffer, 0, position - packet.Length);
                        position -= packet.Length;
                    }
                }
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
        tcpClient?.Dispose();
    }
}
