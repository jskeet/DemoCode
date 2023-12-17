using DigiMixer.DmSeries.Core;

namespace DigiMixer.DmSeries;

public static class DmMessages
{
    public static class Types
    {
        public const string Channels = "MMIX";
    }

    public static class Subtypes
    {
        public const string Channels = "Mixing";
    }

    public static DmMessage KeepAlive { get; } = new DmMessage(
        "EEVT", 0x03010104,
        [
            new DmUInt32Segment([0x0000]),
            new DmUInt32Segment([0x0000]),
            new DmTextSegment("KeepAlive"),
            new DmTextSegment("")
        ]);

    public static DmMessage MeterRequest { get; } = new DmMessage(
        "EEVT", 0x03010104,
        [
            new DmUInt32Segment([0x0000]),
            new DmUInt32Segment([0x0000]),
            new DmTextSegment("RequestMeter"),
            new DmTextSegment("metertype:unicast operation:start target:all interval:50 duration:10000")
        ]);

    public static DmBinarySegment Empty16ByteBinarySegment { get; } =
        new DmBinarySegment(new byte[16]);

    public static DmMessage RequestData(string type, string subtype) => new DmMessage(
        type, flags: 0x01010102, [new DmTextSegment(subtype), new DmBinarySegment([0x80])]);

    public static DmMessage UnrequestData(string type, string subtype) => new DmMessage(
        type, flags: 0x01010102, [new DmTextSegment(subtype), new DmBinarySegment([0x00])]);

    internal static bool IsKeepAlive(DmMessage message) =>
        // We could check more than this, but why bother?
        message.Type == KeepAlive.Type && (message.Flags == 0x03011004 || message.Flags == 0x03010104);
}
