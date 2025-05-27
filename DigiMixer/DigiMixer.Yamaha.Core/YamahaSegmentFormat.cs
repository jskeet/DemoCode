namespace DigiMixer.Yamaha.Core;

public enum YamahaSegmentFormat : byte
{
    Binary = 0x11,
    UInt16 = 0x12,
    UInt32 = 0x14,
    Int32 = 0x24,
    Text = 0x31,
}
