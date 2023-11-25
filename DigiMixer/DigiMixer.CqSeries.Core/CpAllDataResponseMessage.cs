namespace DigiMixer.CqSeries.Core;

public class CqAllDataResponseMessage : CqMessage
{
    public override CqMessageType Type => CqMessageType.AllDataResponse;

    public CqAllDataResponseMessage(byte[] data) : base(CqMessageFormat.VariableLength, data)
    {
    }

    internal CqAllDataResponseMessage(CqMessageFormat format, byte[] data) : base(format, data)
    {
    }
}
