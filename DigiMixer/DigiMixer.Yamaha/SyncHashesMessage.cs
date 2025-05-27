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

    private SyncHashesMessage(YamahaMessage rawMessage) : base(rawMessage)
    {
    }

    public static new SyncHashesMessage? TryParse(YamahaMessage rawMessage)
    {
        if (rawMessage.Segments.Count != 4 ||
            rawMessage.Segments[0] is not YamahaBinarySegment seg0 ||
            rawMessage.Segments[1] is not YamahaTextSegment ||
            rawMessage.Segments[2] is not YamahaBinarySegment seg2 ||
            rawMessage.Segments[3] is not YamahaBinarySegment seg3)
        {
            return null;
        }
        if (seg0.Data.Length != 1 || seg0.Data[0] != 0 ||
            seg2.Data.Length != 16 || seg3.Data.Length != 16)
        {
            return null;
        }
        return new(rawMessage);
    }
}
