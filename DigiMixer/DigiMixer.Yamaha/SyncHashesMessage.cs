using DigiMixer.Yamaha.Core;

namespace DigiMixer.Yamaha;

/// <summary>
/// A message containing a subtype and two hashes: one for the schema and one for the data.
/// </summary>
public sealed class SyncHashesMessage : WrappedMessage
{
    public string Subtype => ((YamahaTextSegment) RawMessage.Segments[1]).Text;
    public ReadOnlySpan<byte> SchemaHash => ((YamahaBinarySegment) RawMessage.Segments[2]).Data;
    public ReadOnlySpan<byte> DataHash => ((YamahaBinarySegment) RawMessage.Segments[3]).Data;

    public SyncHashesMessage(YamahaMessageType type, string subtype, byte flag1, RequestResponseFlag requestResponse, ReadOnlySpan<byte> schemaHash, ReadOnlySpan<byte> dataHash)
        : this(new YamahaMessage(type, flag1, requestResponse,
            [
                new YamahaBinarySegment([0]),
                new YamahaTextSegment(subtype),
                new YamahaBinarySegment(schemaHash),
                new YamahaBinarySegment(dataHash),
            ]))
    {
    }

    private SyncHashesMessage(YamahaMessage rawMessage) : base(rawMessage)
    {
    }

    public static new SyncHashesMessage? TryParse(YamahaMessage rawMessage) =>
        IsSyncHashesMessage(rawMessage) ? new(rawMessage) : null;

    private static bool IsSyncHashesMessage(YamahaMessage rawMessage) =>
        rawMessage.Flag1 == 0x10 &&
        rawMessage.Segments.Count == 4 &&
        rawMessage.Segments[0] is YamahaBinarySegment seg0 &&
        seg0.Data.Length == 1 && seg0.Data[0] == 0 &&
        rawMessage.Segments[1] is YamahaTextSegment &&
        rawMessage.Segments[2] is YamahaBinarySegment seg2 &&
        seg2.Data.Length == 16 &&
        rawMessage.Segments[3] is YamahaBinarySegment seg3 &&
        seg3.Data.Length == 16;
}
