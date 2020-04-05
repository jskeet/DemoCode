// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Model.Fields
{
    /// <summary>
    /// Field which can only be one of a fixed set of values.
    /// </summary>
    public sealed class EnumField : NumericFieldBase
    {
        public IReadOnlyList<string> Values { get; }

        internal EnumField(Parameters common, IReadOnlyList<string> values, int min, int @default)
            : base(common, min, values.Count + min - 1, @default) =>
            Values = values;

        /// <summary>
        /// Determines the value of the field within the context of the specified data.
        /// </summary>
        public string GetValue(FieldContainerData data) => Values[GetRawValue(data) - Min];
    }
}
