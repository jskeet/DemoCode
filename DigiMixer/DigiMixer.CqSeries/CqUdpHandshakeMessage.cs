using DigiMixer.CqSeries.Core;

namespace DigiMixer.CqSeries;

internal sealed class CqUdpHandshakeMessage : CqMessage
{
    public ushort UdpPort => GetUInt16(0);

    public CqUdpHandshakeMessage(ushort udpPort) : base(CqMessageFormat.VariableLength, CqMessageType.UdpHandshake, [(byte) udpPort, (byte) (udpPort >> 8)])
    {
    }

    internal CqUdpHandshakeMessage(CqRawMessage message) : base(message)
    {
    }

    public override string ToString() => $"Type={Type}; UdpPort={UdpPort}";
}
