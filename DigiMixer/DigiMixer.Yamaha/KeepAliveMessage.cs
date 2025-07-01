using DigiMixer.Yamaha.Core;

namespace DigiMixer.Yamaha;

public sealed class KeepAliveMessage : WrappedMessage
{
    public static KeepAliveMessage Request { get; } = new(new (YamahaMessageType.EEVT, 0x01, RequestResponseFlag.Request,
    [
        new YamahaUInt32Segment([0x0000]),
        new YamahaUInt32Segment([0x0000]),
        new YamahaTextSegment("KeepAlive"),
        new YamahaTextSegment("")
    ]));

    public static KeepAliveMessage Response { get; } = new(new (YamahaMessageType.EEVT, 0x01, RequestResponseFlag.Response,
    [
        new YamahaUInt32Segment([0x0000]),
        new YamahaUInt32Segment([0x0000]),
        new YamahaTextSegment("KeepAlive"),
        new YamahaTextSegment("")
    ]));

    private KeepAliveMessage(YamahaMessage rawMessage) : base(rawMessage)
    {
    }

    public static new KeepAliveMessage? TryParse(YamahaMessage rawMessage) =>
        IsKeepAlive(rawMessage) ? (rawMessage.RequestResponse == RequestResponseFlag.Request ? Request : Response)
        : null;

    public static bool IsKeepAlive(YamahaMessage message) =>
       // We could check more than this, but why bother?
       message.Type == Request.RawMessage.Type && message.Flag1 == Request.RawMessage.Flag1;

}
