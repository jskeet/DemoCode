using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.CqSeries.Core;

public sealed class CqControlClient(ILogger logger, string host, int port)
    : TcpMessageProcessingControllerBase<CqRawMessage>(logger, host, port, 65540)
{
    public event EventHandler<CqRawMessage>? MessageReceived;

    protected override Task ProcessMessage(CqRawMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received control message: {message}", message);
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
