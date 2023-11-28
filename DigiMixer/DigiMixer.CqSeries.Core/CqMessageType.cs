namespace DigiMixer.CqSeries.Core;

public enum CqMessageType : byte
{
    UdpHandshake = 0,
    VersionRequest = 1,
    VersionResponse = 2,
    FullDataRequest = 3,
    FullDataResponse = 4,
    KeepAlive = 5,
    ClientInitRequest = 12,
    ClientInitResponse = 13,
    Regular = 7,
}
