namespace DigiMixer.Mackie.Core;

/// <summary>
/// An exception thrown to indicate an error response to a request message.
/// </summary>
public class MackieResponseException(MackieMessage errorMessage)
    : Exception("Received error response to request")
{
    public MackieMessage ErrorMessage { get; } = errorMessage;
}
