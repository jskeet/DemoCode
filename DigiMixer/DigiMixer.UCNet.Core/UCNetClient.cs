using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.UCNet.Core;

public class UCNetClient : TcpMessageProcessingControllerBase<UCNetMessage>
{
    public event EventHandler<UCNetMessage>? MessageReceived;

    public UCNetClient(ILogger logger, string host, int port) :
        base(logger, host, port)
    {
    }

    protected override Task ProcessMessage(UCNetMessage message, CancellationToken cancellationToken)
    {
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
