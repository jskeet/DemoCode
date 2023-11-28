using DigiMixer.Core;
using DigiMixer.CqSeries.Core;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DigiMixer.CqSeries;

public class CqMeterClient : UdpControllerBase, IDisposable
{
    public ushort LocalUdpPort { get; }
    public event EventHandler<CqRawMessage>? MessageReceived;

    private CqMeterClient(ILogger logger, ushort localUdpPort) : base(logger, localUdpPort)
    {
        LocalUdpPort = localUdpPort;
    }

    public CqMeterClient(ILogger logger) : this(logger, FindAvailableUdpPort())
    {
    }

    public async Task SendAsync(CqRawMessage message, IPEndPoint mixerUdpEndPoint, CancellationToken cancellationToken)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending keep-alive message");
        }
        await Send(message.ToByteArray(), mixerUdpEndPoint, cancellationToken);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data)
    {
        if (CqRawMessage.TryParse(data) is not CqRawMessage message)
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
