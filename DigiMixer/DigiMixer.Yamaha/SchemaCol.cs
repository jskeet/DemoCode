using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;

namespace DigiMixer.Yamaha;

public sealed class SchemaCol
{
    public string Name { get; }
    public string Path { get; }
    public ImmutableList<SchemaCol> Cols { get; }
    public ImmutableList<SchemaProperty> Properties { get; }

    /// <summary>
    /// The number of bytes a single instance of this Col takes up.
    /// </summary>
    public int DataLength { get; }

    /// <summary>
    /// The number of types this Col is repeated in the parent.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// The offset of the first instance of this Col, relative to the start of the parent Col.
    /// </summary>
    public int RelativeOffset { get; }

    /// <summary>
    /// The offset of the first instance of this Col, relative to the start of the data.
    /// </summary>
    public int AbsoluteOffset { get; }

    public SchemaCol? Parent { get; }

    private int SchemaLength => 48 + Properties.Count * 32 + Cols.Sum(c => c.SchemaLength);

    internal SchemaCol(SchemaCol? parent, ReadOnlySpan<byte> schema)
    {
        if (schema.Length < 48)
        {
            throw new Exception($"Invalid schema length {schema.Length} for COL: {Encoding.ASCII.GetString(schema)}");
        }
        if (schema[0] != 'C' || schema[1] != 'O' || schema[2] != 'L' || schema[3] != '0')
        {
            throw new ArgumentException($"Unexpected schema data; expected COL, got {Encoding.ASCII.GetString(schema[0..3])}");
        }
        Name = Encoding.ASCII.GetString(schema.Slice(4, 24)).TrimEnd('\0');
        Path = parent is null ? Name : $"{parent.Path}/{Name}";
        RelativeOffset = BinaryPrimitives.ReadInt32LittleEndian(schema.Slice(36, 4));
        AbsoluteOffset = (parent?.AbsoluteOffset ?? 0) + RelativeOffset;
        DataLength = BinaryPrimitives.ReadInt32LittleEndian(schema.Slice(40, 4));
        Count = BinaryPrimitives.ReadInt32LittleEndian(schema.Slice(44, 4));

        var propertiesLength = BinaryPrimitives.ReadInt32LittleEndian(schema.Slice(28, 4));
        var colsLength = BinaryPrimitives.ReadInt32LittleEndian(schema.Slice(32, 4));
        if (schema.Length < 48 + propertiesLength + colsLength)
        {
            throw new ArgumentException($"Invalid length for Col; total length={schema.Length}; properties length={propertiesLength}; cols length={colsLength}");
        }

        var propertiesBuilder = ImmutableList.CreateBuilder<SchemaProperty>();
        int dataOffset = 0;
        for (int i = 0; i < propertiesLength / 32; i++)
        {
            var prop = new SchemaProperty(this, dataOffset, schema.Slice(i * 32 + 48, 32));
            propertiesBuilder.Add(prop);
            dataOffset += prop.Length * prop.Count;
        }
        Properties = propertiesBuilder.ToImmutable();

        var colsBuilder = ImmutableList.CreateBuilder<SchemaCol>();
        int offset = 0;
        while (offset < colsLength)
        {
            var col = new SchemaCol(this, schema[(48 + propertiesLength + offset)..]);
            colsBuilder.Add(col);
            offset += col.SchemaLength;
        }
        Cols = colsBuilder.ToImmutable();
    }
}
