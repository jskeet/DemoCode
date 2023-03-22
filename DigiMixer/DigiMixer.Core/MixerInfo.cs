namespace DigiMixer.Core;

// TODO: Look at what's available for Qu-SB
public sealed class MixerInfo
{
    public string? Model { get; }
    public string? Name { get; }
    public string? Version { get; }

    public MixerInfo(string? model, string? name, string? version)
    {
        Model = model;
        Name = name;
        Version = version;
    }

    public override string ToString() => $"{Name} ({Model}: {Version})";
}
