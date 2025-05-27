using DigiMixer.Yamaha.Core;
using System.Buffers.Binary;
using System.Text;

namespace DigiMixer.Yamaha;

/// <summary>
/// The schema and data for a section, parsed from a <see cref="SectionSchemaAndDataMessage"/>
/// </summary>
public sealed class SectionSchemaAndData
{
    private readonly YamahaBinarySegment segment;
    private readonly int schemaLength;

    public string Name { get; }
    public string SchemaHash { get; }
    public SchemaCol Schema { get; }

    public SectionSchemaAndData(YamahaBinarySegment segment)
    {
        this.segment = segment;
        var segmentData = segment.Data;

        var header = segmentData[..88];
        Name = Encoding.ASCII.GetString(header[8..44]).TrimEnd('\0');
        SchemaHash = Encoding.ASCII.GetString(header[44..76]);
        schemaLength = BinaryPrimitives.ReadInt32LittleEndian(header[80..]);
        var dataLength = BinaryPrimitives.ReadInt32LittleEndian(header[84..]);

        if (schemaLength + dataLength + 88 != segmentData.Length)
        {
            throw new ArgumentException($"Invalid section data length. Segment length: {segmentData.Length}; schema length: {schemaLength}; values length: {dataLength}");
        }

        // For now, assume that there's a single root Col.
        Schema = new SchemaCol(null, segmentData.Slice(88, schemaLength));
    }
}
