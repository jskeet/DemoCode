using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.Yamaha;

public sealed class SchemaProperty
{
    /// <summary>
    /// The name of the property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The full path to the property (via ancestors).
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The type of the property.
    /// </summary>
    public SchemaPropertyType Type { get; }

    /// <summary>
    /// How many times this property is repeated.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Number of bytes this takes up in a Col.
    /// </summary>
    public int Length { get; }

    public SchemaCol Parent { get; }

    /// <summary>
    /// The offset of the first instance of this property, relative to the start of the Col.
    /// </summary>
    public int RelativeOffset { get; }

    /// <summary>
    /// The offset of the first instance of this property, relative to the start of the data.
    /// </summary>
    public int AbsoluteOffset { get; }

    internal SchemaProperty(SchemaCol parent, int offset, ReadOnlySpan<byte> schema)
    {
        if (schema.Length != 32)
        {
            throw new Exception($"Invalid schema length {schema.Length} for property: {schema.Length}");
        }
        if (schema[0] != 'P' || schema[1] != 'R' || schema[2] != ' ')
        {
            throw new ArgumentException($"Unexpected schema data; expected property, got {Encoding.ASCII.GetString(schema[0..3])}");
        }
        Type = (SchemaPropertyType) schema[3];
        Length = BinaryPrimitives.ReadUInt16LittleEndian(schema[4..6]);
        Count = BinaryPrimitives.ReadUInt16LittleEndian(schema[6..8]);
        Name = Encoding.ASCII.GetString(schema.Slice(8, 24)).TrimEnd('\0');
        Path = $"{parent.Path}/{Name}";
        Parent = parent;
        RelativeOffset = offset;
        AbsoluteOffset = offset + parent.AbsoluteOffset;
    }
}
