using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.DmSeries.Core;

public class DmClient : TcpMessageProcessingControllerBase<DmMessage>
{
    private readonly Func<DmMessage, CancellationToken, Task> handler;

    public DmClient(ILogger logger, string host, int port, Func<DmMessage, CancellationToken, Task> handler) :
        base(logger, host, port, DmMessage.TryParse, message => message.Length, bufferSize: 1024 * 1024)
    {
        this.handler = handler;
    }

    public async Task SendAsync(DmMessage message, CancellationToken cancellationToken)
    {
        var data = message.ToByteArray();
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await Send(data, cancellationToken);
    }

    protected override async Task ProcessMessage(DmMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received message: {message}", message);
        await handler(message, cancellationToken);
    }
}
