namespace DigiMixer.CqSeries.Core;

public class CqFullDataResponseMessage : CqMessage
{
    public override CqMessageType Type => CqMessageType.FullDataResponse;

    public CqFullDataResponseMessage(byte[] data) : base(CqMessageFormat.VariableLength, data)
    {
    }

    internal CqFullDataResponseMessage(CqMessageFormat format, byte[] data) : base(format, data)
    {
    }
}
