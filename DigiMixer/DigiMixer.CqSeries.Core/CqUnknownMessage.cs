namespace DigiMixer.CqSeries.Core;

public sealed class CqUnknownMessage : CqMessage
{
    public override CqMessageType Type { get; }

    public CqUnknownMessage(CqMessageFormat format, CqMessageType type, byte[] data) : base(format, data)
    {
        Type = type;
    }
}
