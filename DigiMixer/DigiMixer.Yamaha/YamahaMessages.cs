using DigiMixer.Yamaha.Core;

namespace DigiMixer.Yamaha;

public static class YamahaMessages
{
    public static YamahaMessage KeepAlive { get; } = new(YamahaMessageType.EEVT, 0x03010104,
    [
        new YamahaUInt32Segment([0x0000]),
            new YamahaUInt32Segment([0x0000]),
            new YamahaTextSegment("KeepAlive"),
            new YamahaTextSegment("")
    ]);

    internal static bool IsKeepAlive(YamahaMessage message) =>
        // We could check more than this, but why bother?
        message.Type == KeepAlive.Type && (message.Header == 0x03011004 || message.Header == 0x03010104);
}
