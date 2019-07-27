using System;
using System.Globalization;
using static System.FormattableString;

namespace VDrumExplorer.Data.Fields
{
    public class RangeField : FieldBase, IPrimitiveField
    {
        public int? Min { get; }
        public int? Max { get; }
        public int? Off { get; }
        public int? Divisor { get; }
        public int? Multiplier { get; }
        public int? ValueOffset { get; }
        public string? Suffix { get; }

        public RangeField(FieldPath path, ModuleAddress address, int size, string description,
            int? min, int? max, int? off, int? divisor, int? multiplier, int? valueOffset, string? suffix)
            : base(path, address, size, description) =>
            (Min, Max, Off, Divisor, Multiplier, ValueOffset, Suffix) = (min, max, off, divisor, multiplier, valueOffset, suffix);

        public string GetText(ModuleData data)
        {
            // TODO: Multiplier, divisor etc.
            int value = GetRawValue(data);
            if (value > Max || value < Min)
            {
                throw new InvalidOperationException($"Invalid range value: {value}");
            }
            if (value == Off)
            {
                return "Off";
            }
            value += ValueOffset ?? 0;
            value *= Multiplier ?? 1;
            decimal scaled = value / (Divisor ?? 1m);
            return Invariant($"{scaled}{Suffix}");
        }
    }
}
