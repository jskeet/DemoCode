using DigiMixer.Yamaha.Core;

namespace DigiMixer.Yamaha;

public sealed class MonitorDataMessage : WrappedMessage
{
    public MonitorDataMessage(YamahaMessageType type, RequestResponseFlag requestResponse)
        : this(new(type, 0x4, requestResponse, []))
    {
    }

    private MonitorDataMessage(YamahaMessage rawMessage) : base(rawMessage)
    {
    }

    public static new MonitorDataMessage? TryParse(YamahaMessage rawMessage) =>
        IsMonitorDataMessage(rawMessage) ? new(rawMessage) : null;

    private static bool IsMonitorDataMessage(YamahaMessage rawMessage) =>
        rawMessage.Flag1 == 0x04 && rawMessage.Segments.Count == 0;
}
