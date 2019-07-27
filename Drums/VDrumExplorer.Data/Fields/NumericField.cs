using System;
using static System.FormattableString;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// A field that is normally displayed as a number, with some optional scaling, offseting etc.
    /// </summary>
    public sealed class NumericField : NumericFieldBase
    {
        /// <summary>
        /// Some fields have a single "special" value (e.g. "Off", or "-INF").
        /// If this property is non-null, it provides the raw value and appropriate text.
        /// </summary>
        public (int value, string text)? CustomValueFormatting { get; }
        public string? OffLabel { get; }
        public int? Divisor { get; }
        public int? Multiplier { get; }
        public int? ValueOffset { get; }
        public string? Suffix { get; }

        public NumericField(FieldPath path, ModuleAddress address, int size, string description, int min, int max,
            int? divisor, int? multiplier, int? valueOffset, string? suffix, (int value, string text)? customValueFormatting)
            : base(path, address, size, description, min, max) =>
            (Divisor, Multiplier, ValueOffset, Suffix, CustomValueFormatting) = (divisor, multiplier, valueOffset, suffix, customValueFormatting);

        public override string GetText(ModuleData data)
        {
            int value = GetRawValue(data);
            if (CustomValueFormatting != null && value == CustomValueFormatting.Value.value)
            {
                return CustomValueFormatting.Value.text;
            }
            value += ValueOffset ?? 0;
            value *= Multiplier ?? 1;
            decimal scaled = value / (Divisor ?? 1m);
            return Invariant($"{scaled}{Suffix}");
        }
    }
}
