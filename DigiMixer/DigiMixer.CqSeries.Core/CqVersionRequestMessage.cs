namespace DigiMixer.CqSeries.Core;

public class CqVersionRequestMessage : CqMessage
{
    public override CqMessageType Type => CqMessageType.VersionRequest;

    public CqVersionRequestMessage() : base(CqMessageFormat.VariableLength, [])
    {
    }

    internal CqVersionRequestMessage(CqMessageFormat format, byte[] data) : base(format, data)
    {
    }
}
