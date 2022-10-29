using System.Diagnostics.CodeAnalysis;

namespace DigiMixer.Core;

/// <summary>
/// The level of a meter, e.g. the current output of a channel,
/// with a maximum level of 0dB.
/// </summary>
public struct MeterLevel : IEquatable<MeterLevel>, IComparable<MeterLevel>
{
    public static MeterLevel MinValue { get; } = new MeterLevel(double.NegativeInfinity);
    public static MeterLevel MaxValue { get; } = new MeterLevel(0d);

    public double Value { get; }

    // TODO: ToString, linearize to a given integer scale.
    public MeterLevel(double value) =>
        Value = value;

    public int CompareTo(MeterLevel other) => Value.CompareTo(other.Value);
    public bool Equals(MeterLevel other) => Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is MeterLevel other && this == other;
    public static bool operator ==(MeterLevel left, MeterLevel right) => left.Equals(right);
    public static bool operator !=(MeterLevel left, MeterLevel right) => !left.Equals(right);
    public static bool operator >(MeterLevel left, MeterLevel right) => left.Value > right.Value;
    public static bool operator <(MeterLevel left, MeterLevel right) => left.Value < right.Value;
    public static bool operator >=(MeterLevel left, MeterLevel right) => left.Value >= right.Value;
    public static bool operator <=(MeterLevel left, MeterLevel right) => left.Value <= right.Value;
}
