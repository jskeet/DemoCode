using DigiMixer.AllenAndHeath.Core;

namespace DigiMixer.SqSeries;

internal class SqVersionRequestMessage : SqMessage
{
    public SqVersionRequestMessage() : base(SqMessageType.VersionRequest, [])
    {
    }

    internal SqVersionRequestMessage(AHRawMessage rawMessage) : base(rawMessage)
    {
    }

    public override string ToString() => $"Type={Type}";
}
