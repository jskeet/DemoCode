using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.DmSeries.Core;

public class DmClient(ILogger logger, string host, int port, Func<DmMessage, CancellationToken, Task> handler)
    : TcpMessageProcessingControllerBase<DmMessage>(logger, host, port, bufferSize: 1024 * 1024)
{
    protected override async Task ProcessMessage(DmMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received message: {message}", message);
        await handler(message, cancellationToken);
    }
}
