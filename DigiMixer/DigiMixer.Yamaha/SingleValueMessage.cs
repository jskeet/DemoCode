using DigiMixer.Yamaha.Core;
using System.Collections.Immutable;
using System.Security.Principal;

namespace DigiMixer.Yamaha;

/// <summary>
/// Reports/requests a change to a single value.
/// </summary>
public sealed class SingleValueMessage : WrappedMessage
{
    public string SectionName => ((YamahaTextSegment) RawMessage.Segments[1]).Text;

    /// <summary>
    /// A path navigating from the top of the section schema to the relevant property.
    /// Each value is the index of the property/col within a <see cref="SchemaCol"/>.
    /// </summary>
    public ImmutableList<uint> SchemaPath => ((YamahaUInt32Segment) RawMessage.Segments[4]).Values;

    /// <summary>
    /// The indexes along the path from the root col, e.g. indicating which channel is being represented.
    /// </summary>
    public ImmutableList<uint> SchemaIndexes => ((YamahaUInt32Segment) RawMessage.Segments[5]).Values;

    // We don't know if this is really a client ID yet - it always seems to be 0xa0 or 0xa1.
    public uint ClientId => ((YamahaUInt32Segment) RawMessage.Segments[6]).Values[0];

    public YamahaSegment ValueSegment => RawMessage.Segments[7];

    public SchemaProperty ResolveProperty(SchemaCol rootCol)
    {
        var currentCol = rootCol;
        // Everything up until the last value is a col, then there's a property.
        var colCount = SchemaPath.Count - 1;

        for (int i = 0; i < colCount; i++)
        {
            var schemaIndex = (int) SchemaPath[i];
            var colIndex = schemaIndex - currentCol.Properties.Count;
            if (colIndex < 0 || colIndex >= currentCol.Cols.Count)
            {
                throw new ArgumentException($"Invalid schema path: {string.Join(".", SchemaPath)} at index {i}");
            }
            currentCol = currentCol.Cols[colIndex];
            if (SchemaIndexes[i] >= currentCol.Count)
            {
                throw new ArgumentException($"Invalid schema index {SchemaIndexes[i]} in schema path: {string.Join(".", SchemaPath)} at index {i}");
            }
        }

        var propertyIndex = (int) SchemaPath[colCount];
        if (propertyIndex < 0 || propertyIndex >= currentCol.Properties.Count)
        {
            throw new ArgumentException($"Invalid schema path: {string.Join(".", SchemaPath)} at final index for property");
        }
        var property = currentCol.Properties[propertyIndex];
        if (SchemaIndexes[colCount] >= currentCol.Count)
        {
            throw new ArgumentException($"Invalid schema index {SchemaIndexes[colCount]} in schema path: {string.Join(".", SchemaPath)} for property");
        }
        return property;
    }

    private SingleValueMessage(YamahaMessage rawMessage) : base(rawMessage)
    {
    }

    public static new SingleValueMessage? TryParse(YamahaMessage rawMessage) =>
        IsSingleValueMessage(rawMessage) ? new SingleValueMessage(rawMessage) : null;

    private static bool IsSingleValueMessage(YamahaMessage rawMessage) =>
        rawMessage.Segments.Count == 8 &&
        rawMessage.Flag1 == 0x11 &&
        // From MixingStation, it's a UInt16[*1] - and then the response has text
        // for the fifth segment...
        // rawMessage.Segments[0] is YamahaBinarySegment seg0 &&
        // seg0.Data.Length == 1 && seg0.Data[0] == 0 &&
        rawMessage.Segments[1] is YamahaTextSegment seg1 &&
        rawMessage.Segments[2] is YamahaTextSegment seg2 &&
        seg1.Text == seg2.Text &&
        rawMessage.Segments[3] is YamahaUInt16Segment seg3 &&
        seg3.Values.Count == 1 &&
        rawMessage.Segments[4] is YamahaUInt32Segment seg4 &&
        rawMessage.Segments[5] is YamahaUInt32Segment seg5 &&
        seg4.Values.Count == seg5.Values.Count &&
        seg4.Values.Count == seg3.Values[0] &&
        rawMessage.Segments[6] is YamahaUInt32Segment;
}
