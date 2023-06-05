using System.Diagnostics.CodeAnalysis;

namespace DigiMixer.Core;

/// <summary>
/// Identifier for a channel, as well as whether it's an input or an output.
/// </summary>
/// <remarks>
/// Different mixers may have different "special" channel IDs, e.g. for FX or aux.
/// </remarks>
public struct ChannelId : IEquatable<ChannelId>
{
    public int Value { get; }
    public bool IsInput { get; }
    public bool IsOutput => !IsInput;
    /// <summary>
    /// Returns true if this ID is <see cref="MainOutputLeft"/> or <see cref="MainOutputRight"/>; false otherwise.
    /// </summary>
    public bool IsMainOutput => Equals(MainOutputLeft) || Equals(MainOutputRight);

    /// <summary>
    /// The output channel ID for the left side of the main output.
    /// </summary>
    public static ChannelId MainOutputLeft { get; } = Output(100);
    /// <summary>
    /// The output channel ID for the right side of the main output.
    /// </summary>
    public static ChannelId MainOutputRight { get; } = Output(101);

    private ChannelId(int value, bool input) =>
        (Value, IsInput) = (value, input);

    public static ChannelId Input(int value) => new ChannelId(value, true);
    public static ChannelId Output(int value) => new ChannelId(value, false);

    /// <summary>
    /// Creates a new ChannelId with the given value, and the same input/output aspect
    /// as this ChannelId.
    /// </summary>
    public ChannelId WithValue(int value) => new ChannelId(value, IsInput);

    public bool Equals(ChannelId other) => Value == other.Value && IsInput == other.IsInput;
    public override int GetHashCode() => Value ^ (IsInput ? 0x10000 : 0);
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ChannelId other && this == other;
    public static bool operator ==(ChannelId left, ChannelId right) => left.Equals(right);
    public static bool operator !=(ChannelId left, ChannelId right) => !left.Equals(right);
    public override string ToString() => $"{(IsInput ? "Input": "Output")} Ch{Value}";

    /// <summary>
    /// Returns a text representation of a possibly-stereo-pair, assuming this to be the left-or-mono
    /// channel.
    /// </summary>
    public string ToString(ChannelId? right) =>
        right is null ? ToString() : $"{(IsInput ? "Input" : "Output")} Ch{Value}+{right.Value}";
}
