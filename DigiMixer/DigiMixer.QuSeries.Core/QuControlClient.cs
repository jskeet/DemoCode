using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.QuSeries.Core;

public sealed class QuControlClient : TcpMessageProcessingControllerBase<QuControlMessage>
{
    public event EventHandler<QuControlMessage>? MessageReceived;

    public QuControlClient(ILogger logger, string host, int port) : base(logger, host, port, QuControlMessage.TryParse, message => message.Length)
    {
    }

    public async Task SendAsync(QuControlMessage message, CancellationToken cancellationToken)
    {
        var data = message.ToByteArray();
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await Send(data, cancellationToken);
    }

    protected override void ProcessMessage(QuControlMessage message)
    {
        Logger.LogTrace("Received message: {message}", message);
        MessageReceived?.Invoke(this, message);
    }
}
