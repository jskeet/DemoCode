namespace DigiMixer.TfSeries.Tools;

internal record DecodingOptions(bool SkipKeepAlive, bool DecodeSchemaAndData)
{
    internal static DecodingOptions Simple { get; } = new(false, false);
    internal static DecodingOptions Investigative { get; } = new(true, true);
}