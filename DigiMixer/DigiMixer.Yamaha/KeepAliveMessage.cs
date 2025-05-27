using DigiMixer.Yamaha.Core;

namespace DigiMixer.Yamaha;

public sealed class KeepAliveMessage : WrappedMessage
{
    // TODO: Do we need to worry about the different headers here? Maybe we should have two instances?
    public static KeepAliveMessage Instance { get; } = new(new (YamahaMessageType.EEVT, 0x03010104,
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
        IsKeepAlive(rawMessage) ? Instance : null;

     internal static bool IsKeepAlive(YamahaMessage message) =>
        // We could check more than this, but why bother?
        message.Type == Instance.RawMessage.Type && (message.Header == 0x03011004 || message.Header == 0x03010104);

}
