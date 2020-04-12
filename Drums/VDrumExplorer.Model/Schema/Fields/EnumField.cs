// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// Field which can only be one of a fixed set of values.
    /// </summary>
    public sealed class EnumField : NumericFieldBase
    {
        public IReadOnlyList<string> Values { get; }

        internal IReadOnlyDictionary<string, int> RawNumberByName;

        internal EnumField(Parameters common, IReadOnlyList<string> values, int min, int @default)
            : base(common, min, values.Count + min - 1, @default)
        {
            Values = values;
            RawNumberByName = Values
                .Select((value, index) => (value, index))
                .ToDictionary(pair => pair.value, pair => pair.index + min, StringComparer.Ordinal)
                .AsReadOnly();
        }
    }
}
