using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.UCNet.Core;

public class UCNetClient : TcpMessageProcessingControllerBase<UCNetMessage>
{
    public event EventHandler<UCNetMessage>? MessageReceived;

    public UCNetClient(ILogger logger, string host, int port) :
        base(logger, host, port, UCNetMessage.TryParse, message => message.Length)
    {
    }

    public async Task Send(UCNetMessage message, CancellationToken cancellationToken)
    {
        var data = message.ToByteArray();
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await Send(data, cancellationToken);
    }

    protected override Task ProcessMessage(UCNetMessage message, CancellationToken cancellationToken)
    {
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
