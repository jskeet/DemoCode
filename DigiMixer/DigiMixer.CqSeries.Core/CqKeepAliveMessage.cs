namespace DigiMixer.CqSeries.Core;

public class CqKeepAliveMessage : CqMessage
{
    public override CqMessageType Type => CqMessageType.KeepAlive;

    public CqKeepAliveMessage() : base(CqMessageFormat.VariableLength, [])
    {
    }

    internal CqKeepAliveMessage(CqMessageFormat format, byte[] data) : base(format, data)
    {
    }
}
