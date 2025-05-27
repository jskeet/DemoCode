using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.Yamaha.Core;

public class YamahaClient(ILogger logger, string host, int port, Func<YamahaMessage, CancellationToken, Task> handler)
    : TcpMessageProcessingControllerBase<YamahaMessage>(logger, host, port, bufferSize: 1024 * 1024)
{
    protected override async Task ProcessMessage(YamahaMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received message: {message}", message);
        await handler(message, cancellationToken);
    }
}
