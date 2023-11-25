namespace DigiMixer.CqSeries.Core;

public class CqFullDataRequestMessage : CqMessage
{
    public override CqMessageType Type => CqMessageType.FullDataRequest;

    public CqFullDataRequestMessage() : base(CqMessageFormat.VariableLength, [])
    {
    }

    internal CqFullDataRequestMessage(CqMessageFormat format, byte[] data) : base(format, data)
    {
    }
}
