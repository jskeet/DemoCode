using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.QuSeries.Core;

public sealed class QuControlClient : TcpControllerBase
{
    public event EventHandler<QuControlMessage>? MessageReceived;

    private MessageProcessor<QuControlMessage> processor;

    public QuControlClient(ILogger logger, string host, int port) : base(logger, host, port)
    {
        processor = new MessageProcessor<QuControlMessage>(
            QuControlMessage.TryParse,
            message => message.Length,
            ProcessMessage,
            65540);
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

    protected override void ProcessData(ReadOnlySpan<byte> data) => processor.Process(data);

    private void ProcessMessage(QuControlMessage message)
    {
        Logger.LogTrace("Received message: {message}", message);
        MessageReceived?.Invoke(this, message);
    }
}
