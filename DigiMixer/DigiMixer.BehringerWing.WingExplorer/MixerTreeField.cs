using DigiMixer.BehringerWing.Core;

namespace DigiMixer.BehringerWing.WingExplorer;

public record MixerTreeField(WingNodeDefinition Definition, WingToken? TokenValue, string FullName, string Name)
{
    public WingNodeType Type => Definition.Type;
    public WingNodeUnit Units => Definition.Units;
    public string Value => Type switch
    {
        WingNodeType.String or WingNodeType.StringEnum => TokenValue?.StringValue ?? "",
        WingNodeType.Integer => (TokenValue?.IntegerValue ?? 0).ToString(),
        WingNodeType.FloatEnum or
        WingNodeType.FaderLevel or
        WingNodeType.LinearFloat =>
            (TokenValue?.Float32Value ?? float.NaN).ToString(),
        _ => "Unknown"
    };

    public override string ToString() => FullName;
}
