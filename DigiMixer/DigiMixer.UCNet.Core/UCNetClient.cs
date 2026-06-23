using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.UCNet.Core;

public class UCNetClient(ILogger logger, string host, int port)
    : TcpMessageProcessingControllerBase<UCNetMessage>(logger, host, port)
{
    public event EventHandler<UCNetMessage>? MessageReceived;

    protected override Task ProcessMessage(UCNetMessage message, CancellationToken cancellationToken)
    {
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
