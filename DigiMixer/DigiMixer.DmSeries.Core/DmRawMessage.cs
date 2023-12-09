using DigiMixer.Core;
using System.Runtime.InteropServices;
using System.Text;

namespace DigiMixer.DmSeries.Core;

public class DmRawMessage
{
    public string Type { get; }
    private ReadOnlyMemory<byte> data;

    public ReadOnlySpan<byte> Data => data.Span;

    /// <summary>
    /// Message length including header and data.
    /// </summary>
    public int Length => data.Length + 8;

    public DmRawMessage(string type, ReadOnlyMemory<byte> data)
    {
        Type = type;
        this.data = data;
    }

    public static DmRawMessage? TryParse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8)
        {
            return null;
        }
        int bodyLength = BigEndian.ReadInt32(data[4..8]);
        if (data.Length < bodyLength + 8)
        {
            return null;
        }
        var type = Encoding.ASCII.GetString(data[0..4]).Trim('\0');
        return new DmRawMessage(type, data.Slice(8, bodyLength).ToArray());
    }

    internal ReadOnlyMemory<byte> ToByteArray()
    {
        var ret = new byte[Length];
        var span = ret.AsSpan();
        Encoding.ASCII.GetBytes(Type, ret);
        BigEndian.WriteInt32(span.Slice(4), Data.Length);
        Data.CopyTo(span.Slice(8));
        return ret;
    }

    public override string ToString() => $"{Type}: {Data.Length} bytes";
}
