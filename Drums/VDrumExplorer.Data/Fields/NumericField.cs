// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Globalization;
using static System.FormattableString;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// A field that is normally displayed as a number, with some optional scaling, offseting etc.
    /// </summary>
    public class NumericField : NumericFieldBase
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

        internal NumericField(FieldBase.Parameters common, int min, int max, int @default,
            int? divisor, int? multiplier, int? valueOffset, string? suffix, (int value, string text)? customValueFormatting)
            : base(common, min, max, @default) =>
            (Divisor, Multiplier, ValueOffset, Suffix, CustomValueFormatting) =
            (divisor, multiplier, valueOffset, suffix, customValueFormatting);

        public override string GetText(FixedContainer context, ModuleData data)
        {
            int value = GetRawValue(context, data);
            if (CustomValueFormatting != null && value == CustomValueFormatting.Value.value)
            {
                return CustomValueFormatting.Value.text;
            }
            decimal scaled = ScaleRawValueForFormatting(value);
            return Invariant($"{scaled}{Suffix}");
        }

        public override bool TrySetText(FixedContainer context, ModuleData data, string text)
        {
            if (CustomValueFormatting != null && text == CustomValueFormatting.Value.text)
            {
                SetRawValue(context, data, CustomValueFormatting.Value.value);
                return true;
            }
            if (Suffix != null && text.EndsWith(Suffix))
            {
                text = text.Substring(0, text.Length - Suffix.Length);
            }
            if (!decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return false;
            }
            // Reverse the scaling...
            decimal value = parsed * (Divisor ?? 1m);
            value /= Multiplier ?? 1m;
            value -= ValueOffset ?? 0;
            if (value < int.MinValue || value > int.MaxValue)
            {
                return false;
            }
            int candidateRawValue = (int) value;
            if (candidateRawValue < Min || candidateRawValue > Max)
            {
                return false;
            }
            
            // Check that this raw candidate value is actually reasonable.
            // This avoids problems such as when the divisor is 2, but the user enters 1.3.
            decimal rescaled = ScaleRawValueForFormatting(candidateRawValue);
            if (rescaled == parsed)
            {
                SetRawValue(context, data, candidateRawValue);
                return true;
            }
            else
            {
                return false;
            }
        }

        private decimal ScaleRawValueForFormatting(int value)
        {
            value += ValueOffset ?? 0;
            value *= Multiplier ?? 1;
            return value / (Divisor ?? 1m);
        }
    }
}
