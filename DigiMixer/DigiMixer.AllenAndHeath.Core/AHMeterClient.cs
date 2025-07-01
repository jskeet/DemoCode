using DigiMixer.AllenAndHeath.Core;
using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Net;

namespace AllenAndHeath.Core;

public class AHMeterClient : UdpControllerBase, IDisposable
{
    private readonly MemoryPool<byte> SendingPool = MemoryPool<byte>.Shared;

    public ushort LocalUdpPort { get; }
    public event EventHandler<AHRawMessage>? MessageReceived;

    private AHMeterClient(ILogger logger, ushort localUdpPort) : base(logger, localUdpPort)
    {
        LocalUdpPort = localUdpPort;
    }

    public AHMeterClient(ILogger logger) : this(logger, FindAvailableUdpPort())
    {
    }

    public async Task SendAsync(AHRawMessage message, IPEndPoint mixerUdpEndPoint, CancellationToken cancellationToken)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Sending keep-alive message");
        }
        using var memoryOwner = SendingPool.Rent(message.Length);
        var memory = memoryOwner.Memory[..message.Length];
        message.CopyTo(memory.Span);
        await Send(memory, mixerUdpEndPoint, cancellationToken);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data)
    {
        if (AHRawMessage.TryParse(data) is not AHRawMessage message)
        {
            return;
        }
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Received meter message: {message}", message);
        }
        MessageReceived?.Invoke(this, message);
    }
}
