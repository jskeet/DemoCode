using DigiMixer.Core;
using DigiMixer.CqSeries.Core;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DigiMixer.CqSeries;

public class CqMeterClient : UdpControllerBase, IDisposable
{
    private static readonly byte[] KeepAlive = new CqKeepAliveMessage().ToByteArray();

    public ushort LocalUdpPort { get; }
    public event EventHandler<CqMessage>? MessageReceived;

    private CqMeterClient(ILogger logger, ushort localUdpPort) : base(logger, localUdpPort)
    {
        LocalUdpPort = localUdpPort;
    }

    public CqMeterClient(ILogger logger) : this(logger, FindAvailableUdpPort())
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
        if (CqMessage.TryParse(data) is not CqMessage message)
        {
            return;
        }
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Received meter message: {message}", message);
        }
        MessageReceived?.Invoke(this, message);
    }
}
