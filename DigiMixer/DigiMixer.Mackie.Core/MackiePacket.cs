using System.Text;

namespace DigiMixer.Mackie.Core;

public sealed class MackiePacket
{
    private static readonly byte[] emptyBody = new byte[0];
    private const byte Header0 = 0xab;

    public MackieCommand Command { get; }
    public MackiePacketType Type { get; }
    public byte Sequence { get; }

    public int Length => Body.Length == 0 ? 8 : Body.Length + 12;
    public MackiePacketBody Body { get; }

    public MackiePacket(byte sequence, MackiePacketType type, MackieCommand command, MackiePacketBody body)
    {
        Sequence = sequence;
        Type = type;
        Command = command;
        Body = body;
    }

    public static MackiePacket? TryParse(byte[] buffer, int index, int length)
    {
        if (length < 8 || buffer[index] != Header0)
        {
            return null;
        }
        byte seq = buffer[index + 1];
        int chunkCount = (buffer[index + 2] << 8) + buffer[index + 3];
        MackiePacketType type = (MackiePacketType) buffer[index + 4];
        MackieCommand command = (MackieCommand) buffer[index + 5];

        // If we have any chunks, then there's the 8 byte header, the 4 byte chunks, then a 4 byte checksum.
        if (chunkCount != 0 && length < chunkCount * 4 + 12)
        {
            return null;
        }
        // Note: we don't validate the checksum in the final 4 bytes...
        var body = chunkCount == 0 ? MackiePacketBody.Empty : new MackiePacketBody(buffer.AsSpan().Slice(index + 8, chunkCount * 4));
        return new MackiePacket(seq, type, command, body);
    }

    /// <summary>
    /// Creates a response packet responding to this packet, which must be a request packet.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal MackiePacket CreateResponse(MackiePacketBody body)
    {
        if (Type != MackiePacketType.Request)
        {
            throw new InvalidOperationException($"{nameof(CreateResponse)} can only be called on request packets");
        }
        return new MackiePacket(Sequence, MackiePacketType.Response, Command, body);
    }

    internal byte[] ToByteArray()
    {
        var body = Body.InNetworkOrder();

        byte[] packet = new byte[Length];
        packet[0] = Header0;
        packet[1] = Sequence;
        packet[2] = (byte) (Body.ChunkCount >> 8);
        packet[3] = (byte) (Body.ChunkCount >> 0);
        packet[4] = (byte) Type;
        packet[5] = (byte) Command;

        ushort headerChecksum = 0xffff;
        for (int i = 0; i < 6; i++)
        {
            headerChecksum -= packet[i];
        }
        packet[6] = (byte) (headerChecksum >> 8);
        packet[7] = (byte) (headerChecksum >> 0);

        if (body.Length != 0)
        {
            body.Data.CopyTo(packet.AsSpan().Slice(8));
            uint bodyChecksum = 0xffff_ffff;
            for (int i = 0; i < Body.Length; i++)
            {
                bodyChecksum -= Body.Data[i];
            }
            packet[body.Length + 8] = (byte) (bodyChecksum >> 24);
            packet[body.Length + 9] = (byte) (bodyChecksum >> 16);
            packet[body.Length + 10] = (byte) (bodyChecksum >> 8);
            packet[body.Length + 11] = (byte) (bodyChecksum >> 0);
        }
        return packet;
    }

    // TODO: Include the hex of the body?
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append($"Seq: {Sequence:x2}; Type: {Type}; Command: {Command}; Body: {Body.Length} bytes");
        if (Body.Length > 0)
        {
            var data = Body.InNetworkOrder().Data;
            builder.Append(":");
            for (int i = 0; i < 8 && i < data.Length; i += 4)
            {
                if (i != 0)
                {
                    builder.Append(" ");
                }
                builder.AppendFormat(" {0:x2} {1:x2} {2:x2} {3:x2}", data[i], data[i + 1], data[i + 2], data[i + 3]);
            }
            if (Body.Length > 8)
            {
                builder.Append("...");
            }
        }
        return builder.ToString();
    }
}
