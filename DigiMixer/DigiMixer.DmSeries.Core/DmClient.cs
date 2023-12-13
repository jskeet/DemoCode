using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.DmSeries.Core;

public class DmClient : TcpControllerBase
{
    public event EventHandler<DmMessage>? MessageReceived;

    private MessageProcessor<DmMessage> processor;

    public DmClient(ILogger logger, string host, int port) : base(logger, host, port)
    {
        processor = new MessageProcessor<DmMessage>(
            DmMessage.TryParse,
            message => message.Length,
            ProcessMessage,
            // TODO: See how big messages really are
            1024 * 1024);
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

    protected override void ProcessData(ReadOnlySpan<byte> data) => processor.Process(data);

    private void ProcessMessage(DmMessage message)
    {
        Logger.LogTrace("Received message: {message}", message);
        MessageReceived?.Invoke(this, message);
    }
}
