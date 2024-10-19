using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.BehringerWing.Core;

public sealed class WingNodeDefinition
{
    public int ParentHash { get; }
    public int NodeHash { get; }
    public int NodeIndex { get; }
    public string Name { get; }
    public string LongName { get; }
    // TODO: Rework this
    public int Flags { get; }
    // TODO: Enums etc

    private WingNodeDefinition(int parentHash, int nodeHash, int nodeIndex, string name, string longName, int flags)
    {
        ParentHash = parentHash;
        NodeHash = nodeHash;
        NodeIndex = nodeIndex;
        Name = name;
        LongName = longName;
        Flags = flags;
    }

    internal static WingNodeDefinition Parse(ReadOnlySpan<byte> data)
    {
        var parentHash = BinaryPrimitives.ReadInt32BigEndian(data);
        var nodeHash = BinaryPrimitives.ReadInt32BigEndian(data[4..]);
        var nodeIndex = BinaryPrimitives.ReadInt16BigEndian(data[6..]);
        int nameLength = data[8];
        string name = WingConstants.Encoding.GetString(data.Slice(9, nameLength));
        int longNameLength = data[9 + nameLength];
        string longName = WingConstants.Encoding.GetString(data.Slice(10 + nameLength, longNameLength));
        var flags = BinaryPrimitives.ReadInt16BigEndian(data[(10 + nameLength + longNameLength)..]);
        // TODO: Post-flags enums etc
        return new(parentHash, nodeHash, nodeIndex, name, longName, flags);
    }

    public int CopyTo(Span<byte> span)
    {
        throw new NotImplementedException();
    }
}
