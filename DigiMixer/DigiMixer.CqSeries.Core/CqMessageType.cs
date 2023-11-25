namespace DigiMixer.CqSeries.Core;

public enum CqMessageType : byte
{
    Handshake = 0,
    Type1 = 1,
    Type2 = 2,
    AllDataRequest = 3,
    AllDataResponse = 4,
    KeepAlive = 5,
    Type12 = 12,
    Regular = 7,
}
