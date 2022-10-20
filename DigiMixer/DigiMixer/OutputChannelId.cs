using System.Diagnostics.CodeAnalysis;

namespace DigiMixer;

/// <summary>
/// Identifier for an output channel (bus, main, aux etc).
/// This is just a wrapper around an integer, but it improves
/// readability and avoids confusion between input and output channels.
/// </summary>
/// <remarks>
/// Different mixers may have different "special" channel IDs, e.g. for FX or aux.
/// </remarks>
public struct OutputChannelId : IEquatable<OutputChannelId>
{
    public int Value { get; }

    public OutputChannelId(int value) =>
        Value = value;

    public bool Equals(OutputChannelId other) => Value == other.Value;
    public override int GetHashCode() => Value;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is OutputChannelId other && Equals(this, other);
    public static bool operator ==(OutputChannelId left, OutputChannelId right) => left.Equals(right);
    public static bool operator !=(OutputChannelId left, OutputChannelId right) => !left.Equals(right);
    public override string ToString() => $"Ch{Value}";

    public static (OutputChannelId, OutputChannelId?) Pair(int left, int? right) =>
        (new OutputChannelId(left), right is int x ? new OutputChannelId(x) : null);
}
