using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DigiMixer.DmSeries.Core;

public class DmClient : TcpControllerBase
{
    public event EventHandler<DmRawMessage>? MessageReceived;

    private MessageProcessor<DmRawMessage> processor;

    public DmClient(ILogger logger, string host, int port) : base(logger, host, port)
    {
        processor = new MessageProcessor<DmRawMessage>(
            DmRawMessage.TryParse,
            message => message.Length,
            ProcessMessage,
            // TODO: See how big messages really are
            1024 * 1024);
    }

    public async Task SendAsync(DmRawMessage message, CancellationToken cancellationToken)
    {
        var data = message.ToByteArray();
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending message: {message}", message);
        }
        await Send(data, cancellationToken);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data) => processor.Process(data);

    private void ProcessMessage(DmRawMessage message)
    {
        Logger.LogTrace("Received message: {message}", message);
        MessageReceived?.Invoke(this, message);
    }
}

