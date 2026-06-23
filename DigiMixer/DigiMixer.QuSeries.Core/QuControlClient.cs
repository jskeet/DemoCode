using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.QuSeries.Core;

public sealed class QuControlClient(ILogger logger, string host, int port) : TcpMessageProcessingControllerBase<QuControlMessage>(logger, host, port)
{
    public event EventHandler<QuControlMessage>? MessageReceived;

    protected override Task ProcessMessage(QuControlMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received message: {message}", message);
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
