using DigiMixer.Core;
using System.Text;

namespace DigiMixer.Mackie.Core;

public sealed class MackieMessage
{
    private static readonly byte[] emptyBody = new byte[0];
    private const byte Header0 = 0xab;

    public MackieCommand Command { get; }
    public MackieMessageType Type { get; }
    public byte Sequence { get; }

    public int Length => Body.Length == 0 ? 8 : Body.Length + 12;
    public MackieMessageBody Body { get; }

    public MackieMessage(byte sequence, MackieMessageType type, MackieCommand command, MackieMessageBody body)
    {
        Sequence = sequence;
        Type = type;
        Command = command;
        Body = body;
    }

    public static MackieMessage? TryParse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8 || data[0] != Header0)
        {
            return null;
        }
        byte seq = data[1];
        int chunkCount = BigEndian.ReadInt16(data.Slice(2));
        MackieMessageType type = (MackieMessageType) data[4];
        MackieCommand command = (MackieCommand) data[5];

        // If we have any chunks, then there's the 8 byte header, the 4 byte chunks, then a 4 byte checksum.
        if (chunkCount != 0 && data.Length < chunkCount * 4 + 12)
        {
            return null;
        }
        // Note: we don't validate the checksum in the final 4 bytes...
        var body = chunkCount == 0 ? MackieMessageBody.Empty : new MackieMessageBody(data.Slice(8, chunkCount * 4));
        return new MackieMessage(seq, type, command, body);
    }

    /// <summary>
    /// Creates a response message responding to this message, which must be a request message.
    /// </summary>
    internal MackieMessage CreateResponse(MackieMessageBody body)
    {
        if (Type != MackieMessageType.Request)
        {
            throw new InvalidOperationException($"{nameof(CreateResponse)} can only be called on request message");
        }
        return new MackieMessage(Sequence, MackieMessageType.Response, Command, body);
    }

    internal byte[] ToByteArray()
    {
        var body = Body.InNetworkOrder();

        byte[] message = new byte[Length];
        var span = message.AsSpan();
        message[0] = Header0;
        message[1] = Sequence;
        BigEndian.WriteInt16(span.Slice(2), (short) Body.ChunkCount);
        message[4] = (byte) Type;
        message[5] = (byte) Command;

        ushort headerChecksum = 0xffff;
        for (int i = 0; i < 6; i++)
        {
            headerChecksum -= message[i];
        }
        BigEndian.WriteUInt16(span.Slice(6), headerChecksum);

        if (body.Length != 0)
        {
            body.Data.CopyTo(message.AsSpan().Slice(8));
            uint bodyChecksum = 0xffff_ffff;
            for (int i = 0; i < Body.Length; i++)
            {
                bodyChecksum -= Body.Data[i];
            }
            BigEndian.WriteUInt32(span.Slice(body.Length + 8), bodyChecksum);
        }
        return message;
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
