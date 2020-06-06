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
        public int Min { get; }
        public int Max { get; }
        public int Default { get; }

        private protected NumericFieldBase(FieldContainer? parent, FieldParameters common, int min, int max, int @default)
            : base(parent, common) =>
            (Min, Max, Default) = (min, max, @default);
    }
}
