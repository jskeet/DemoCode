using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.CqSeries.Core;

public sealed class CqControlClient : TcpControllerBase
{
    public event EventHandler<CqMessage>? MessageReceived;

    private MessageProcessor<CqMessage> processor;

    public CqControlClient(ILogger logger, string host, int port) : base(logger, host, port)
    {
        processor = new MessageProcessor<CqMessage>(
            CqMessage.TryParse,
            message => message.Length,
            ProcessMessage,
            65540);
    }

    public async Task SendAsync(CqMessage message, CancellationToken cancellationToken)
    {
        var data = message.ToByteArray();
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await Send(data, cancellationToken);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data) => processor.Process(data);

    private void ProcessMessage(CqMessage message)
    {
        Logger.LogTrace("Received message: {message}", message);
        MessageReceived?.Invoke(this, message);
    }
}
