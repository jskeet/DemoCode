using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.DmSeries.Core;

public class DmClient : TcpMessageProcessingControllerBase<DmMessage>
{
    public event EventHandler<DmMessage>? MessageReceived;

    public DmClient(ILogger logger, string host, int port) :
        base(logger, host, port, DmMessage.TryParse, message => message.Length, bufferSize: 1024 * 1024)
    {
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

    protected override Task ProcessMessage(DmMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received message: {message}", message);
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
