namespace DigiMixer.TfSeries.Tools;

internal record DecodingOptions(bool SkipKeepAlive, bool DecodeSchema, bool DecodeData, bool ShowAllSegments)
{
    internal static DecodingOptions Simple { get; } = new(false, false, false, false);
    internal static DecodingOptions Investigative { get; } = new(false, false, false, true);
}