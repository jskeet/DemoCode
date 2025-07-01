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

    public static new SectionSchemaAndDataMessage? TryParse(YamahaMessage rawMessage) =>
        IsSectionSchemaAndDataMessage(rawMessage) ? new(rawMessage) : null;

    private static bool IsSectionSchemaAndDataMessage(YamahaMessage rawMessage) =>
        rawMessage.Flag1 == 0x14 &&
        rawMessage.Segments.Count == 9 &&
        rawMessage.Segments[0] is YamahaBinarySegment seg0 &&
        seg0.Data.Length == 1 && seg0.Data[0] == 0 &&
        rawMessage.Segments[1] is YamahaTextSegment seg1 &&
        rawMessage.Segments[2] is YamahaTextSegment seg2 &&
        seg1.Text == seg2.Text &&
        rawMessage.Segments[3] is YamahaUInt16Segment seg3 &&
        seg3.Values.Count == 1 && seg3.Values[0] == 0 &&
        rawMessage.Segments[4] is YamahaUInt32Segment seg4 &&
        seg4.Values.Count == 0 &&
        rawMessage.Segments[5] is YamahaUInt32Segment seg5 &&
        seg5.Values.Count == 0 &&
        rawMessage.Segments[6] is YamahaUInt32Segment seg6 &&
        seg6.Values.Count == 1 && seg6.Values[0] == 0x000000f0 &&
        rawMessage.Segments[7] is YamahaBinarySegment seg7 &&
        seg7.Data.Length >= 88 &&
        rawMessage.Segments[8] is YamahaBinarySegment seg8 &&
        seg8.Data.Length == 0;
}
