namespace DigiMixer.Mackie.Core;

/// <summary>
/// An exception thrown to indicate an error response to a request packet.
/// </summary>
public class MackieResponseException : Exception
{
    public MackiePacket ErrorPacket { get; }

    public MackieResponseException(MackiePacket errorPacket) : base("Received error response to request")
    {
        ErrorPacket = errorPacket;
    }
}
