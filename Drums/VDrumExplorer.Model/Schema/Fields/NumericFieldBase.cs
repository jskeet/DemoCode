// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// Abstract base class for fields based on a numeric value in a given range.
    /// Concrete subclasses provide formatting.
    /// </summary>
    public abstract class NumericFieldBase : FieldBase
    {
        /// <summary>
        /// The raw (unformatted) minimum value.
        /// </summary>
        public int Min => NumericBaseParameters.Min;

        /// <summary>
        /// The raw (unformatted) maximum value.
        /// </summary>
        public int Max => NumericBaseParameters.Max;
        public int Default => NumericBaseParameters.Default;
        internal NumericCodec Codec => NumericBaseParameters.Codec;

        protected NumericFieldBaseParameters NumericBaseParameters { get; }

        private protected NumericFieldBase(
            FieldContainer? parent,
            FieldParameters common,
            NumericFieldBaseParameters numericBaseParameters)
            : base(parent, common) =>
            NumericBaseParameters = numericBaseParameters;            

        protected class NumericFieldBaseParameters
        {
            internal int Min { get; }
            internal int Max { get; }
            internal int Default { get; }
            internal NumericCodec Codec { get; }

            internal NumericFieldBaseParameters(int min, int max, int @default, NumericCodec codec) =>
                (Min, Max, Default, Codec) = (min, max, @default, codec);
        }
    }
}
