namespace DigiMixer.CqSeries.Core;

public class CqAllDataRequestMessage : CqMessage
{
    public override CqMessageType Type => CqMessageType.AllDataRequest;

    public CqAllDataRequestMessage() : base(CqMessageFormat.VariableLength, [])
    {
    }

    internal CqAllDataRequestMessage(CqMessageFormat format, byte[] data) : base(format, data)
    {
    }
}
