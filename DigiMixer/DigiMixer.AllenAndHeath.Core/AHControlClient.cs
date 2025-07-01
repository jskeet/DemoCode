using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.AllenAndHeath.Core;

public sealed class AHControlClient : TcpMessageProcessingControllerBase<AHRawMessage>
{
    public event EventHandler<AHRawMessage>? MessageReceived;

    public AHControlClient(ILogger logger, string host, int port) : base(logger, host, port, bufferSize: 65540)
    {
    }

    protected override Task ProcessMessage(AHRawMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received control message: {message}", message);
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
