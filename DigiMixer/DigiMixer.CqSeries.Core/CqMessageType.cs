namespace DigiMixer.CqSeries.Core;

public enum CqMessageType : byte
{
    Handshake = 0,
    VersionRequest = 1,
    VersionResponse = 2,
    FullDataRequest = 3,
    FullDataResponse = 4,
    KeepAlive = 5,
    Type12 = 12,
    Regular = 7,
}
