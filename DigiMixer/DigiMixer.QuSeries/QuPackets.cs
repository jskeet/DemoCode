using DigiMixer.QuSeries.Core;

namespace DigiMixer.QuSeries;

internal static class QuPackets
{
    internal static QuControlPacket RequestControlPackets { get; } = QuControlPacket.Create(type: 4, new byte[]
    { 
        0x13, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0x9f, 0x0f, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0xe0, 0x03, 0xc0,
        0xff, 0xff, 0xff, 0x7f
    });

    internal static QuControlPacket RequestFullData { get; } = QuControlPacket.Create(type: 4, new byte[] { 0x02, 0x00 });
    internal static QuControlPacket RequestVersionInformation { get; } = QuControlPacket.Create(type: 4, new byte[] { 0x00, 0x01 });
    internal static QuControlPacket RequestNameInformation { get; } = QuControlPacket.Create(type: 4, new byte[] { 0x08, 0x00 });

    internal const byte FullDataType = 0x06;
    internal const byte NameInformationType = 0x0c;
    internal const byte VersionInformationType = 0x01;
}
