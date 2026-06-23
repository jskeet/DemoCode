using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.AllenAndHeath.Core;

public sealed class AHControlClient(ILogger logger, string host, int port)
    : TcpMessageProcessingControllerBase<AHRawMessage>(logger, host, port, bufferSize: 65540)
{
    public event EventHandler<AHRawMessage>? MessageReceived;

    protected override Task ProcessMessage(AHRawMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received control message: {message}", message);
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
