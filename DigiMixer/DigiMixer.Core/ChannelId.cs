using System.Diagnostics.CodeAnalysis;

namespace DigiMixer.Core;

/// <summary>
/// Identifier for a channel, as well as whether it's an input or an output.
/// </summary>
/// <remarks>
/// Different mixers may have different "special" channel IDs, e.g. for FX or aux.
/// </remarks>
public struct ChannelId
{
    public int Value { get; }
    public bool IsInput { get; }
    public bool IsOutput => !IsInput;

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
}
