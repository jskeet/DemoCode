using System.Diagnostics.CodeAnalysis;

namespace DigiMixer;

/// <summary>
/// Identifier for an input channel.
/// This is just a wrapper around an integer, but it improves
/// readability and avoids confusion between input and output channels.
/// </summary>
/// <remarks>
/// Different mixers may have different "special" channel IDs, e.g. for FX or aux.
/// </remarks>
public struct InputChannelId
{
    public int Value { get; }

    public InputChannelId(int value) =>
        Value = value;

    public bool Equals(InputChannelId other) => Value == other.Value;
    public override int GetHashCode() => Value;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is InputChannelId other && Equals(this, other);
    public static bool operator ==(InputChannelId left, InputChannelId right) => left.Equals(right);
    public static bool operator !=(InputChannelId left, InputChannelId right) => !left.Equals(right);
}
