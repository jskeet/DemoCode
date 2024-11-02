namespace DigiMixer.BehringerWing.Core;

public enum WingMeterType : byte
{
    InputChannel = 0xa0,
    Aux = 0xa1,
    Bus = 0xa2,
    Main = 0xa3,
    Matrix = 0xa4,
    Dca = 0xa5,
    Fx = 0xa6,
    SourceDevice = 0xa7,
    OutputDevice = 0xa8,
    InputChannelV2 = 0xab,
    AuxV2 = 0xac,
    BusV2 = 0xad,
    MainV2 = 0xae,
    MatrixV2 = 0xaf
}
