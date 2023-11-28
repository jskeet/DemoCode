using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.CqSeries.Core;

public sealed class CqControlClient : TcpControllerBase
{
    public event EventHandler<CqRawMessage>? MessageReceived;

    private MessageProcessor<CqRawMessage> processor;

    public CqControlClient(ILogger logger, string host, int port) : base(logger, host, port)
    {
        processor = new MessageProcessor<CqRawMessage>(
            CqRawMessage.TryParse,
            message => message.Length,
            ProcessMessage,
            65540);
    }

    public async Task SendAsync(CqRawMessage message, CancellationToken cancellationToken)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await Send(message.ToByteArray(), cancellationToken);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data) => processor.Process(data);

    private void ProcessMessage(CqRawMessage message)
    {
        Logger.LogTrace("Received control message: {message}", message);
        MessageReceived?.Invoke(this, message);
    }
}
