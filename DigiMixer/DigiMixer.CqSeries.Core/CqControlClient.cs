using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.CqSeries.Core;

public sealed class CqControlClient : TcpMessageProcessingControllerBase<CqRawMessage>
{
    public event EventHandler<CqRawMessage>? MessageReceived;

    public CqControlClient(ILogger logger, string host, int port) : base(logger, host, port, CqRawMessage.TryParse, message => message.Length, 65540)
    {
    }

    public async Task SendAsync(CqRawMessage message, CancellationToken cancellationToken)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await Send(message.ToByteArray(), cancellationToken);
    }

    protected override Task ProcessMessage(CqRawMessage message, CancellationToken cancellationToken)
    {
        Logger.LogTrace("Received control message: {message}", message);
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
