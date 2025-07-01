using DigiMixer.AllenAndHeath.Core;

namespace DigiMixer.SqSeries;

/// <summary>
/// A simple request message with no additional data.
/// </summary>
public class SqSimpleRequestMessage : SqMessage
{
    internal SqSimpleRequestMessage(SqMessageType type) : base(type, Array.Empty<byte>())
    {
    }

    internal SqSimpleRequestMessage(AHRawMessage rawMessage) : base(rawMessage)
    {
    }
}
