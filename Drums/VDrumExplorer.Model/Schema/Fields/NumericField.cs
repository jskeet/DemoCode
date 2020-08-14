// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Physical;
using static System.FormattableString;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// A field that is normally displayed as a number, with some optional scaling, offseting etc.
    /// </summary>
    public sealed class NumericField : NumericFieldBase
    {
        private readonly NumericFieldParameters numericFieldParameters;

        private NumericField(FieldContainer? parent, FieldParameters common,
            NumericFieldBaseParameters numericBaseParameters, NumericFieldParameters numericFieldParameters)
            : base(parent, common, numericBaseParameters) =>
            this.numericFieldParameters = numericFieldParameters;

        internal NumericField(FieldContainer? parent, FieldParameters common, int min, int max, int @default, NumericCodec codec,
            int? divisor, int? multiplier, int? valueOffset, string? suffix, (int value, string text)? customValueFormatting)
            : this(parent, common,
                  new NumericFieldBaseParameters(min, max, @default, codec),
                  new NumericFieldParameters(divisor, multiplier, valueOffset, suffix, customValueFormatting))
        {
        }

        internal override FieldBase WithParent(FieldContainer parent) =>
            new NumericField(parent, Parameters, NumericBaseParameters, numericFieldParameters);


        internal string FormatRawValue(int rawValue) => numericFieldParameters.FormatRawValue(rawValue);

        private class NumericFieldParameters
        {
            public (int value, string text)? CustomValueFormatting { get; }
            public string? OffLabel { get; }
            public int? Divisor { get; }
            public int? Multiplier { get; }
            public int? ValueOffset { get; }
            public string? Suffix { get; }

            internal NumericFieldParameters (int? divisor, int? multiplier, int? valueOffset, string? suffix, (int value, string text)? customValueFormatting) =>
                (Divisor, Multiplier, ValueOffset, Suffix, CustomValueFormatting) =
                (divisor, multiplier, valueOffset, suffix, customValueFormatting);

            internal string FormatRawValue(int rawValue)
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
    }
}
