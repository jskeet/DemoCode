namespace DigiMixer.Mackie.Core;

/// <summary>
/// An exception thrown to indicate an error response to a request message.
/// </summary>
public class MackieResponseException : Exception
{
    public MackieMessage ErrorMessage { get; }

    public MackieResponseException(MackieMessage errorMessage) : base("Received error response to request")
    {
        ErrorMessage = errorMessage;
    }
}
