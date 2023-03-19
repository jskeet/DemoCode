using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.UCNet.Core;

public class UCNetClient : TcpControllerBase
{
    public event EventHandler<UCNetMessage>? MessageReceived;

    private readonly MessageProcessor<UCNetMessage> processor;

    public UCNetClient(ILogger logger, string host, int port) : base(logger, host, port)
    {
        processor = new MessageProcessor<UCNetMessage>(
            UCNetMessage.TryParse,
            message => message.Length,
            message => MessageReceived?.Invoke(this, message),
            65542);
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

    protected override void ProcessData(ReadOnlySpan<byte> data) => processor.Process(data);
}
