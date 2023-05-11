namespace DigiMixer.Mackie.Core;

public enum MackiePacketType : byte
{
    Request = 0,
    Response = 1,
    Error = 5, // Seen when requesting info that isn't available.
    Broadcast = 8
}
