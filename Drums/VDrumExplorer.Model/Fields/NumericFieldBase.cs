// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Model.Fields
{
    /// <summary>
    /// Abstract base class for fields based on a numeric value in a given range.
    /// Concrete subclasses provide formatting.
    /// </summary>
    public abstract class NumericFieldBase : FieldBase
    {
        public int Min { get; }
        public int Max { get; }
        public int Default { get; }

        private protected NumericFieldBase(Parameters common, int min, int max, int @default)
            : base(common) =>
            (Min, Max, Default) = (min, max, @default);

        protected int GetRawValueUnvalidated(FieldContainerData data) => data.ReadInt32(Offset, Size);

        protected int GetRawValue(FieldContainerData data)
        {
            var value = GetRawValueUnvalidated(data);
            if (value < Min || value > Max)
            {
                throw new InvalidOperationException($"Invalid range value: {value}");
            }
            return value;
        }

        internal override bool ValidateData(FieldContainerData data, out string? error)
        {
            var rawValue = GetRawValueUnvalidated(data);
            if (rawValue < Min || rawValue > Max)
            {
                error = $"Invalid raw value {rawValue}. Expected range: [{Min}-{Max}]";
                return false;
            }
            error = null;
            return true;
        }
    }
}
