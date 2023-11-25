namespace DigiMixer.CqSeries.Core;

public sealed class CqHandshakeMessage : CqMessage
{
    public override CqMessageType Type => CqMessageType.Handshake;

    public ushort UdpPort => GetUInt16(0);

    public CqHandshakeMessage(ushort udpPort) : base(CqMessageFormat.VariableLength, [(byte) udpPort, (byte) (udpPort >> 8)])
    {
    }

    internal CqHandshakeMessage(CqMessageFormat format, byte[] data) : base(format, data)
    {
    }

    public override string ToString() => $"Type={Type}; UdpPort={UdpPort}";
}
