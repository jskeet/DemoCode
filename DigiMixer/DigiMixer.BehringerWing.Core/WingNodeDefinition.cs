using System.Buffers.Binary;

namespace DigiMixer.BehringerWing.Core;

public sealed class WingNodeDefinition
{
    /// <summary>
    /// A fake node definition for convenience.
    /// </summary>
    public static WingNodeDefinition Root { get; } = new WingNodeDefinition(0, 0, 0, "Root", "", 0);

    public uint ParentHash { get; }
    public uint NodeHash { get; }
    public ushort NodeIndex { get; }
    public string Name { get; }
    public string LongName { get; }
    public ushort Flags { get; }
    // TODO: Enums etc

    public WingNodeType Type => (WingNodeType) ((Flags & 0xf0) >> 4);
    public WingNodeUnit Units => (WingNodeUnit) (Flags & 0x0f);
    public bool IsReadOnly => (Flags & 0x100) != 0;
    public bool IsNode => Type == WingNodeType.Node;

    private WingNodeDefinition(uint parentHash, uint nodeHash, ushort nodeIndex, string name, string longName, ushort flags)
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
        var parentHash = BinaryPrimitives.ReadUInt32BigEndian(data);
        var nodeHash = BinaryPrimitives.ReadUInt32BigEndian(data[4..]);
        var nodeIndex = BinaryPrimitives.ReadUInt16BigEndian(data[8..]);
        int nameLength = data[10];
        string name = WingConstants.Encoding.GetString(data.Slice(11, nameLength));
        int longNameLength = data[11 + nameLength];
        string longName = WingConstants.Encoding.GetString(data.Slice(12 + nameLength, longNameLength));
        var flags = BinaryPrimitives.ReadUInt16BigEndian(data[(12 + nameLength + longNameLength)..]);
        // TODO: Post-flags enums etc
        return new(parentHash, nodeHash, nodeIndex, name, longName, flags);
    }

    // We don't current send node definitions, so we don't need to be able to serialize them.
    internal int CopyTo(Span<byte> span) => throw new NotImplementedException();
}
