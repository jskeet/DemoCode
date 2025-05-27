using DigiMixer.Yamaha.Core;

namespace DigiMixer.Yamaha;

public sealed class SectionSchemaAndDataMessage : WrappedMessage
{
    public SectionSchemaAndData Data { get; }
    public string Subtype => ((YamahaTextSegment) RawMessage.Segments[2]).Text;

    private SectionSchemaAndDataMessage(YamahaMessage rawMessage) : base(rawMessage)
    {
        Data = new((YamahaBinarySegment) RawMessage.Segments[7]);
    }

    public static new SectionSchemaAndDataMessage? TryParse(YamahaMessage rawMessage)
    {
        var segments = rawMessage.Segments;
        if (segments.Count != 9 ||
            segments[0] is not YamahaBinarySegment seg0 ||
            segments[1] is not YamahaTextSegment seg1 ||
            segments[2] is not YamahaTextSegment seg2 ||
            segments[3] is not YamahaUInt16Segment seg3 ||
            segments[4] is not YamahaUInt32Segment seg4 ||
            segments[5] is not YamahaUInt32Segment seg5 ||
            segments[6] is not YamahaUInt32Segment seg6 ||
            segments[7] is not YamahaBinarySegment seg7 ||
            segments[8] is not YamahaBinarySegment seg8)
        {
            return null;
        }

        // Basic shape validation...
        if (seg0.Data.Length != 1 || seg0.Data[0] != 0 ||
            seg1.Text != seg2.Text ||
            seg3.Values.Count != 1 || seg3.Values[0] != 0 ||
            seg4.Values.Count != 0 ||
            seg5.Values.Count != 0 ||
            seg6.Values.Count != 1 || seg6.Values[0] != 0x000000f0 ||
            seg7.Data.Length < 88 ||
            seg8.Data.Length != 0)
        {
            return null;
        }
        return new(rawMessage);
    }
}
