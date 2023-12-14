using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.CqSeries.Core;

public sealed class CqControlClient : TcpMessageProcessingControllerBase<CqRawMessage>
{
    public event EventHandler<CqRawMessage>? MessageReceived;

    public CqControlClient(ILogger logger, string host, int port) : base(logger, host, port, 65540)
    {
    }

    protected override Task ProcessMessage(CqRawMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received control message: {message}", message);
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
