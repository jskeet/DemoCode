using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DigiMixer.QuSeries.Core;

public class QuMeterClient : UdpControllerBase, IDisposable
{
    // TODO: Check this is correct for all clients.
    private static readonly byte[] KeepAlive = new byte[] { 0x7f, 0x25, 0, 0 };

    public int LocalUdpPort { get; }
    public event EventHandler<QuGeneralPacket>? PacketReceived;

    private QuMeterClient(ILogger logger, int localUdpPort) : base(logger, localUdpPort)
    {
        LocalUdpPort = localUdpPort;
    }

    public QuMeterClient(ILogger logger) : this(logger, FindAvailableUdpPort())
    {
    }

    public async Task SendKeepAliveAsync(IPEndPoint mixerUdpEndPoint, CancellationToken cancellationToken)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending keep-alive packet");
        }
        await Send(KeepAlive, mixerUdpEndPoint, cancellationToken);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data)
    {
        if (QuControlPacket.TryParse(data) is not QuGeneralPacket packet)
        {
            return;
        }
        if (Logger.IsEnabled(LogLevel.Trace) && packet.HasNonZeroData())
        {
            Logger.LogTrace("Received packet: {packet}", packet);
        }
        PacketReceived?.Invoke(this, packet);
    }
}
