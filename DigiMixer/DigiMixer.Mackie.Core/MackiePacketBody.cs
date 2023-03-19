using System.Text;

namespace DigiMixer.Mackie.Core;

/// <summary>
/// The body of a Mackie protocol packet.
/// </summary>
public sealed class MackiePacketBody
{
    /// <summary>
    /// An empty packet body.
    /// </summary>
    public static MackiePacketBody Empty { get; } = new MackiePacketBody(new byte[0]);

    /// <summary>
    /// Just a clear way of expressing a lack of packet body, for
    /// <see cref="MackieController.MapCommand(MackieCommand, Func{MackiePacket, MackiePacketBody?})"/>
    /// methods.
    /// </summary>
    public static MackiePacketBody? Null { get; } = null;

    private byte[] data;
    public ReadOnlySpan<byte> Data => data.AsSpan();
    public int Length => Data.Length;

    /// <summary>
    /// Number of chunks in the data. Each chunk is 4 bytes long.
    /// </summary>
    public int ChunkCount => Length / 4;

    public bool IsNetworkOrder { get; }

    /// <summary>
    /// Returns this packet body in network order, i.e. ready for <see cref="Data"/> to be
    /// included in a packet.
    /// </summary>
    /// <returns></returns>
    public MackiePacketBody InNetworkOrder() => IsNetworkOrder ? this : Reverse();

    /// <summary>
    /// Returns this packet body in host order, i.e. ready to be read byte-by-byte sequentially.
    /// (This is primarily important for reading strings.)
    /// </summary>
    public MackiePacketBody InSequentialOrder() => !IsNetworkOrder ? this : Reverse();

    private MackiePacketBody Reverse()
    {
        byte[] newData = new byte[data.Length];
        for (int i = 0; i < data.Length; i += 4)
        {
            newData[i] = data[i + 3];
            newData[i + 1] = data[i + 2];
            newData[i + 2] = data[i + 1];
            newData[i + 3] = data[i];
        }
        return new MackiePacketBody(newData, !IsNetworkOrder);
    }

    // Note: this constructor does *not* clone the data, or validate its length.
    private MackiePacketBody(byte[] data, bool isNetworkOrder)
    {
        this.data = data;
        IsNetworkOrder = isNetworkOrder;
    }
    /// <summary>
    /// Creates a packet with the given data, assumed to be in network order.
    /// The data is cloned on construction.
    /// </summary>
    public MackiePacketBody(byte[] data) : this(data.ToArray(), true)
    {
        if (data.Length % 4 != 0)
        {
            throw new ArgumentException("Data length must be a multiple of 4");
        }
    }

    /// <summary>
    /// Creates a packet with the given data, assumed to be in network order.
    /// The data is cloned on construction.
    /// </summary>
    public MackiePacketBody(ReadOnlySpan<byte> data) : this(data.ToArray())
    {
    }

    /// <summary>
    /// Reads an unsigned integer from the specified 4-byte chunk.
    /// The order of the body representation is taken into account when converting
    /// from the binary data.
    /// </summary>
    public uint GetUInt32(int chunk)
    {
        uint raw = BitConverter.ToUInt32(data, chunk * 4);
        return !IsNetworkOrder
            ? raw
            : (raw >> 24 & 0xff) << 0 |
              (raw >> 16 & 0xff) << 8 |
              (raw >> 8 & 0xff) << 16 |
              (raw >> 0 & 0xff) << 24;
    }

    /// <summary>
    /// Reads a signed integer from the specified 4-byte chunk.
    /// The order of the body representation is taken into account when converting
    /// from the binary data.
    /// </summary>
    public int GetInt32(int chunk) => (int) GetUInt32(chunk);

    /// <summary>
    /// Reads a 32-bit floating point value from the specified 4-byte chunk.
    /// The order of the body representation is taken into account when converting
    /// from the binary data.
    /// </summary>
    public float GetSingle(int chunk) => BitConverter.Int32BitsToSingle(GetInt32(chunk));

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(IsNetworkOrder ? "(N)" : "(S)");
        for (int i = 0; i < data.Length; i += 4)
        {
            if (i != 0)
            {
                builder.Append(" ");
            }
            builder.AppendFormat(" {0:x2} {1:x2} {2:x2} {3:x2}", data[i], data[i + 1], data[i + 2], data[i + 3]);
        }
        return builder.ToString();
    }
}
