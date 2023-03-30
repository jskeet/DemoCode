namespace DigiMixer.Core;

public sealed record MixerInfo(string? Model, string? Name, string? Version)
{
    public override string ToString() => $"{Name} ({Model}: {Version})";

    public static MixerInfo Empty { get; } = new MixerInfo(null, null, null);
}
