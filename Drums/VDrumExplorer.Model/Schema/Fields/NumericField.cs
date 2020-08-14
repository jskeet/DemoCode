// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using VDrumExplorer.Model.Schema.Physical;
using static System.FormattableString;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// A field that is normally displayed as a number, with some optional scaling, offseting etc.
    /// </summary>
    public sealed class NumericField : NumericFieldBase
    {
        private readonly INumericFieldFormatter numericFieldFormatter;

        private NumericField(FieldContainer? parent, FieldParameters common,
            NumericFieldBaseParameters numericBaseParameters, INumericFieldFormatter numericFieldFormatter)
            : base(parent, common, numericBaseParameters) =>
            this.numericFieldFormatter = numericFieldFormatter;

        internal NumericField(FieldContainer? parent, FieldParameters common, int min, int max, int @default, NumericCodec codec,
            int? divisor, int? multiplier, int? valueOffset, string? suffix, (int value, string text)? customValueFormatting)
            : this(parent, common,
                  new NumericFieldBaseParameters(min, max, @default, codec),
                  new ComputedValueFormatter(divisor, multiplier, valueOffset, suffix, customValueFormatting))
        {
        }

        internal NumericField(FieldContainer? parent, FieldParameters common, int min, int max, int @default, NumericCodec codec, IReadOnlyList<string> values)
            : this(parent, common, new NumericFieldBaseParameters(min, max, @default, codec), new CustomValueFormatter(values, min, max))
        {
        }

        internal override FieldBase WithParent(FieldContainer parent) =>
            new NumericField(parent, Parameters, NumericBaseParameters, numericFieldFormatter);

        internal string FormatRawValue(int rawValue) => numericFieldFormatter.FormatRawValue(rawValue);

        private interface INumericFieldFormatter
        {
            string FormatRawValue(int rawValue);
        }

        private class ComputedValueFormatter : INumericFieldFormatter
        {
            public (int value, string text)? CustomValueFormatting { get; }
            public string? OffLabel { get; }
            public int? Divisor { get; }
            public int? Multiplier { get; }
            public int? ValueOffset { get; }
            public string? Suffix { get; }

            internal ComputedValueFormatter(int? divisor, int? multiplier, int? valueOffset, string? suffix, (int value, string text)? customValueFormatting) =>
                (Divisor, Multiplier, ValueOffset, Suffix, CustomValueFormatting) =
                (divisor, multiplier, valueOffset, suffix, customValueFormatting);

            public string FormatRawValue(int rawValue)
            {
                if (CustomValueFormatting is (int customValue, string customValueText) && rawValue == customValue)
                {
                    return customValueText;
                }

                decimal scaled = ScaleRawValueForFormatting(rawValue);
                return Invariant($"{scaled}{Suffix}");
            }

            private decimal ScaleRawValueForFormatting(int value)
            {
                value += ValueOffset ?? 0;
                value *= Multiplier ?? 1;
                return value / (Divisor ?? 1m);
            }
        }

        private class CustomValueFormatter : INumericFieldFormatter
        {
            private readonly int min;
            private readonly IReadOnlyList<string> values;

            internal CustomValueFormatter(IReadOnlyList<string> values, int min, int max)
            {
                (this.values, this.min) = (values, min);
                if (values.Count != max - min + 1)
                {
                    throw new ArgumentException($"Expected {max - min + 1} values; got {values.Count}");
                }
            }

            public string FormatRawValue(int rawValue) => values[rawValue - min];
        }
    }
}
