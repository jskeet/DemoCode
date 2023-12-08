namespace DigiMixer.CqSeries.Core;

public enum CqMessageType : byte
{
    UdpHandshake = 0,
    VersionRequest = 1,
    VersionResponse = 2,
    FullDataRequest = 3,
    FullDataResponse = 4,
    KeepAlive = 5,
    Regular = 7,
    InputMeters = 8,
    OutputMeters = 9,
    // TODO: Work out what the other meters mean.
    Meter10 = 10,
    ClientInitRequest = 12,
    ClientInitResponse = 13,
    Meter23 = 23,
    Meter24 = 24
}
