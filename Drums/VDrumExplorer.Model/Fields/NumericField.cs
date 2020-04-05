// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Model.Fields
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

        internal NumericField(Parameters common, int min, int max, int @default,
            int? divisor, int? multiplier, int? valueOffset, string? suffix, (int value, string text)? customValueFormatting)
            : base(common, min, max, @default) =>
            (Divisor, Multiplier, ValueOffset, Suffix, CustomValueFormatting) =
            (divisor, multiplier, valueOffset, suffix, customValueFormatting);
    }
}
