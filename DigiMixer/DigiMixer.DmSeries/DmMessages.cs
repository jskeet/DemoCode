using DigiMixer.DmSeries.Core;

namespace DigiMixer.DmSeries;

public static class DmMessages
{
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
}
