namespace DigiMixer.Mackie;

public enum MackieCommand : byte
{
    KeepAlive = 0x01,
    ClientHandshake = 0x03,
    FirmwareInfo = 0x04,
    ChannelInfoControl = 0x06,
    GeneralInfo = 0x0e,
    ChannelValues = 0x13,
    BroadcastControl = 0x15,
    MeterLayout = 0x16,
    ChannelName = 0x18
}
