using DigiMixer.Yamaha.Core;
using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.Yamaha;

/// <summary>
/// The schema and data for a section, parsed from a <see cref="SectionSchemaAndDataMessage"/>
/// </summary>
public sealed class SectionSchemaAndData
{
    private const int HeaderLength = 88;

    private readonly YamahaBinarySegment segment;
    private readonly int schemaLength;

    public string Name { get; }
    public string SchemaHash { get; }
    public SchemaCol Schema { get; }

    public string GetString(SchemaProperty property, int additionalOffset = 0)
    {
        var offset = GetOffset(property, additionalOffset);
        return Encoding.ASCII.GetString(segment.Data.Slice(offset, property.Length)).TrimEnd('\0');
    }

    public short GetInt16(SchemaProperty property, int additionalOffset = 0)
    {
        var offset = GetOffset(property, additionalOffset);
        return BinaryPrimitives.ReadInt16LittleEndian(segment.Data[offset..]);
    }

    public int GetInt32(SchemaProperty property, int additionalOffset = 0)
    {
        var offset = GetOffset(property, additionalOffset);
        return BinaryPrimitives.ReadInt32LittleEndian(segment.Data[offset..]);
    }

    public ushort GetUInt16(SchemaProperty property, int additionalOffset = 0)
    {
        var offset = GetOffset(property, additionalOffset);
        return BinaryPrimitives.ReadUInt16LittleEndian(segment.Data[offset..]);
    }

    public uint GetUInt32(SchemaProperty property, int additionalOffset = 0)
    {
        var offset = GetOffset(property, additionalOffset);
        return BinaryPrimitives.ReadUInt32LittleEndian(segment.Data[offset..]);
    }

    public byte GetUInt8(SchemaProperty property, int additionalOffset = 0)
    {
        var offset = GetOffset(property, additionalOffset);
        return segment.Data[offset];
    }

    public sbyte GetInt8(SchemaProperty property, int additionalOffset = 0)
    {
        var offset = GetOffset(property, additionalOffset);
        return (sbyte) segment.Data[offset];
    }

    private int GetOffset(SchemaProperty property, int additionalOffset = 0) => HeaderLength + schemaLength + property.AbsoluteOffset + additionalOffset;

    public SectionSchemaAndData(YamahaBinarySegment segment)
    {
        this.segment = segment;
        var segmentData = segment.Data;

        var header = segmentData[..HeaderLength];
        Name = Encoding.ASCII.GetString(header[8..44]).TrimEnd('\0');
        SchemaHash = Encoding.ASCII.GetString(header[44..76]);
        schemaLength = BinaryPrimitives.ReadInt32LittleEndian(header[80..]);
        var dataLength = BinaryPrimitives.ReadInt32LittleEndian(header[84..]);

        if (schemaLength + dataLength + HeaderLength != segmentData.Length)
        {
            throw new ArgumentException($"Invalid section data length. Segment length: {segmentData.Length}; schema length: {schemaLength}; values length: {dataLength}");
        }

        // For now, assume that there's a single root Col.
        Schema = new SchemaCol(null, segmentData.Slice(HeaderLength, schemaLength));
    }
}
