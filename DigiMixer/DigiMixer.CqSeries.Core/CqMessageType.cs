namespace DigiMixer.CqSeries.Core;

public enum CqMessageType : byte
{
    Handshake = 0,
    Type1 = 1,
    Type2 = 2,
    FullDataRequest = 3,
    FullDataResponse = 4,
    KeepAlive = 5,
    Type12 = 12,
    Regular = 7,
}
