namespace DigiMixer.SqSeries;

public enum SqMessageType : byte
{
    UdpHandshake = 0,
    VersionRequest = 1,
    VersionResponse = 2,
    FullDataRequest = 3,
    FullDataResponse = 4,
    ClientInitRequest = 11,
    ClientInitResponse = 12,
    UsersRequest = 20,
    UsersResponse = 21,
    Type13Request = 13,
    Type15Request = 15,
    Type17Request = 17,
}
