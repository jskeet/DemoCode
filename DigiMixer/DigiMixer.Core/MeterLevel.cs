using System.Diagnostics.CodeAnalysis;

namespace DigiMixer.Core;

/// <summary>
/// The level of a meter, e.g. the current output of a channel,
/// with a maximum level of 0dB. The range is 0.0 to 1.0.
/// </summary>
public struct MeterLevel : IEquatable<MeterLevel>, IComparable<MeterLevel>
{
    public static MeterLevel MinValue { get; } = new MeterLevel(0d);
    public static MeterLevel MaxValue { get; } = new MeterLevel(1d);

    public double Value { get; }

    // TODO: ToString, linearize to a given integer scale.
    public MeterLevel(double value) =>
        Value = value;

    /// <summary>
    /// Converts from decibel levels (non-positive dB)
    /// to a value that spaces the following ranges equally:
    /// - -5dB to 0dB     => 500 to 600
    /// - -10dB to -5dB   => 400 to 500
    /// - -20dB to -10dB  => 300 to 400
    /// - -30dB to -20dB  => 200 to 300
    /// - -50dB to -30dB  => 100 to 200
    /// - -75dB to -50dB  => 0 to 100
    /// Anything lower than -75dB ends up with a value of 0.
    /// 
    /// The value of 0-600 is then scaled to 0-1.
    /// </summary>
    public static MeterLevel FromDb(double db)
    {
        double x = db switch
        {
            >= 0 => 600,
            >= -5f => (db + 5f) * (100f / 5f) + 500,
            >= -10f => (db + 10f) * (100f / 5f) + 400,
            >= -20f => (db + 20f) * (100f / 10f) + 300,
            >= -30f => (db + 30f) * (100f / 10f) + 200,
            >= -50f => (db + 50f) * (100f / 20f) + 100,
            >= -75f => (db + 75f) * (100f / 25) + 0,
            _ => 0f
        };
        return new MeterLevel(x / 600.0);
    }

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

    public override string ToString() => Value.ToString();
}
