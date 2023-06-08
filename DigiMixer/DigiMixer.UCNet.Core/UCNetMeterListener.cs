using DigiMixer.Core;
using DigiMixer.UCNet.Core.Messages;
using Microsoft.Extensions.Logging;

namespace DigiMixer.UCNet.Core;

public class UCNetMeterListener : UdpControllerBase, IDisposable
{
    public int LocalPort { get; }

    public event EventHandler<Meter16Message>? MessageReceived;

    public UCNetMeterListener(ILogger logger, int port) : base(logger, port)
    {
        LocalPort = port;
    }

    public UCNetMeterListener(ILogger logger) : this(logger, FindAvailableUdpPort())
    {
    }

    protected override void ProcessData(ReadOnlySpan<byte> data)
    {
        var message = UCNetMessage.TryParseUdp(data);
        switch (message)
        {
            case null:
                Logger.LogWarning("Received UDP packet length {length} which isn't a full message", data.Length);
                break;
            case Meter16Message meters:
                MessageReceived?.Invoke(this, meters);
                break;
            default:
                Logger.LogWarning("Received UDP packet with unexpected meter type {type}", message.Type);
                break;
        }
    }
}
