using System.Buffers.Binary;

namespace DigiMixer.UCNet.Core.Messages;

public class UdpMetersMessage(int meterPort, MessageMode mode = MessageMode.UdpMeters) : UCNetMessage(mode)
{
    public int MeterPort => meterPort;

    public override MessageType Type => MessageType.UdpMeters;

    protected override int BodyLength => 8;

    protected override void WriteBody(Span<byte> span) => BinaryPrimitives.WriteUInt16LittleEndian(span, (ushort) MeterPort);

    internal static UdpMetersMessage FromRawBody(MessageMode mode, ReadOnlySpan<byte> body) =>
        new(BinaryPrimitives.ReadUInt16LittleEndian(body), mode);

    public override string ToString() => $"UdpMeters: Port={MeterPort}";
}
