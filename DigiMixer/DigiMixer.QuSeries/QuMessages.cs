using DigiMixer.Core;
using DigiMixer.QuSeries.Core;
using System.Buffers.Binary;

namespace DigiMixer.QuSeries;

internal static class QuMessages
{
    internal static QuControlMessage RequestControlMessages { get; } = QuControlMessage.Create(type: 4, new byte[]
    {
        0x13, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0x9f, 0x0f, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0xe0, 0x03, 0xc0,
        0xff, 0xff, 0xff, 0x7f
    });

    internal static QuControlMessage RequestFullData { get; } = QuControlMessage.Create(type: 4, new byte[] { 0x02, 0x00 });
    internal static QuControlMessage RequestVersionInformation { get; } = QuControlMessage.Create(type: 4, new byte[] { 0x00, 0x01 });
    internal static QuControlMessage RequestNetworkInformation { get; } = QuControlMessage.Create(type: 4, new byte[] { 0x08, 0x00 });

    internal const byte FullDataType = 0x06;
    internal const byte NetworkInformationType = 0x0c;
    internal const byte VersionInformationType = 0x01;
    internal const byte InputMeterType = 0x23;
    internal const byte OutputMeterType = 0x24;

    internal static QuControlMessage InitialHandshakeRequest(int localUdpPort) =>
        QuControlMessage.Create(type: 0, new byte[] { (byte) (localUdpPort & 0xff), (byte) (localUdpPort >> 8) });

    internal static bool IsInitialHandshakeResponse(QuControlMessage message, out ushort mixerUdpPort)
    {
        if (message is QuGeneralMessage { Type: 0 } response && response.Data.Length == 2)
        {
            mixerUdpPort = BinaryPrimitives.ReadUInt16LittleEndian(response.Data);
            return true;
        }
        else
        {
            mixerUdpPort = 0;
            return false;
        }
    }
}
