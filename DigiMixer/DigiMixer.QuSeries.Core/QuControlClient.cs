using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.QuSeries.Core;

public sealed class QuControlClient : TcpMessageProcessingControllerBase<QuControlMessage>
{
    public event EventHandler<QuControlMessage>? MessageReceived;

    public QuControlClient(ILogger logger, string host, int port) : base(logger, host, port)
    {
    }

    protected override Task ProcessMessage(QuControlMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received message: {message}", message);
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
