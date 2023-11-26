namespace DigiMixer.CqSeries.Core;

public class CqVersionResponseMessage : CqMessage
{
    public override CqMessageType Type => CqMessageType.VersionResponse;

    public string Version => "FIXME";

    public CqVersionResponseMessage(byte[] data) : base(CqMessageFormat.VariableLength, data)
    {
    }

    internal CqVersionResponseMessage(CqMessageFormat format, byte[] data) : base(format, data)
    {
    }
}
