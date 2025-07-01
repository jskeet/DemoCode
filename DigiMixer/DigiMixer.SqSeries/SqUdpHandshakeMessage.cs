using DigiMixer.AllenAndHeath.Core;

namespace DigiMixer.SqSeries;

internal sealed class SqUdpHandshakeMessage : SqMessage
{
    public ushort UdpPort => GetUInt16(0);

    public SqUdpHandshakeMessage(ushort udpPort) : base(SqMessageType.UdpHandshake, [(byte) udpPort, (byte) (udpPort >> 8)])
    {
    }

    internal SqUdpHandshakeMessage(AHRawMessage message) : base(message)
    {
    }

    public override string ToString() => $"Type={Type}; UdpPort={UdpPort}";
}
