using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.QuSeries.Core;

public sealed class QuControlClient : TcpControllerBase
{
    public event EventHandler<QuControlPacket>? PacketReceived;

    private MessageProcessor<QuControlPacket> processor;

    public QuControlClient(ILogger logger, string host, int port) : base(logger, host, port)
    {
        processor = new MessageProcessor<QuControlPacket>(
            QuControlPacket.TryParse,
            packet => packet.Length,
            ProcessPacket,
            65540);
    }

    public async Task SendAsync(QuControlPacket packet, CancellationToken cancellationToken)
    {
        var data = packet.ToByteArray();
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending packet: {packet}", packet);
        }
        await Send(data, cancellationToken);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data) => processor.Process(data);

    private void ProcessPacket(QuControlPacket packet)
    {
        Logger.LogTrace("Received packet: {packet}", packet);
        PacketReceived?.Invoke(this, packet);
    }
}
