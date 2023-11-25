using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DigiMixer.QuSeries.Core;

public class QuMeterClient : UdpControllerBase, IDisposable
{
    // TODO: Check this is correct for all clients.
    private static readonly byte[] KeepAlive = new byte[] { 0x7f, 0x25, 0, 0 };

    public ushort LocalUdpPort { get; }
    public event EventHandler<QuGeneralMessage>? MessageReceived;

    private QuMeterClient(ILogger logger, ushort localUdpPort) : base(logger, localUdpPort)
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
            Logger.LogTrace("Sending keep-alive message");
        }
        await Send(KeepAlive, mixerUdpEndPoint, cancellationToken);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data)
    {
        if (QuControlMessage.TryParse(data) is not QuGeneralMessage message)
        {
            return;
        }
        if (Logger.IsEnabled(LogLevel.Trace) && message.HasNonZeroData())
        {
            Logger.LogTrace("Received message: {message}", message);
        }
        MessageReceived?.Invoke(this, message);
    }
}
