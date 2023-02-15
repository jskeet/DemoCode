namespace DigiMixer.UCNet.Core;

public enum MessageMode : uint
{
    UdpMeters = 0x00_65_00_00,
    ClientRequest = 0x00_65_00_6a,
    Compressed = 0x00_6a_00_65,
    MixerUpdate = 0x00_6b_00_65,
    FileRequest = 0x00_65_00_6b
}
