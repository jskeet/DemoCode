using DigiMixer.Core;
using Microsoft.Extensions.Logging;

namespace DigiMixer.BehringerWing.Core;

public class WingMeterClient : UdpControllerBase, IDisposable
{
    public ushort LocalUdpPort { get; }
    public event EventHandler<WingMeterMessage>? MessageReceived;

    private WingMeterClient(ILogger logger, ushort localUdpPort) : base(logger, localUdpPort)
    {
        LocalUdpPort = localUdpPort;
    }

    public WingMeterClient(ILogger logger) : this(logger, FindAvailableUdpPort())
    {
    }

    protected override void ProcessData(ReadOnlySpan<byte> data)
    {
        if (WingMeterMessage.TryParse(data) is not WingMeterMessage response)
        {
            return;
        }
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Received meter message: {message}", response);
        }
        MessageReceived?.Invoke(this, response);
    }
}
