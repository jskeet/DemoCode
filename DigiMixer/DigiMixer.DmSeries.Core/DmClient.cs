using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.DmSeries.Core;

public class DmClient : TcpMessageProcessingControllerBase<DmMessage>
{
    private readonly Func<DmMessage, CancellationToken, Task> handler;

    public DmClient(ILogger logger, string host, int port, Func<DmMessage, CancellationToken, Task> handler) :
        base(logger, host, port, bufferSize: 1024 * 1024)
    {
        this.handler = handler;
    }

    protected override async Task ProcessMessage(DmMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received message: {message}", message);
        await handler(message, cancellationToken);
    }
}
