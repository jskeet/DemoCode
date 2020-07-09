// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// Field which can only be one of a fixed set of values.
    /// </summary>
    public sealed class EnumField : NumericFieldBase
    {
        /// <summary>
        /// The valid values, in display order, but with no indication of the specific values involved.
        /// </summary>
        public IReadOnlyList<string> Values { get; }

        /// <summary>
        /// The raw number associated with each value. (Primarily public for the purposes of diagnostics.)
        /// </summary>
        public IReadOnlyDictionary<string, int> RawNumberByName { get; }
        internal IReadOnlyDictionary<int, string> NameByRawNumber { get; }

        internal EnumField(FieldContainer parent, EnumField other)
            : base(parent, other.Parameters, other.Min, other.Max, other.Default) =>
            (Values, RawNumberByName, NameByRawNumber) = (other.Values, other.RawNumberByName, other.NameByRawNumber);

        internal EnumField(FieldContainer? parent, FieldParameters common, IReadOnlyList<(int number, string value)> numberValuePairs, int @default)
            : base(parent, common, numberValuePairs.Select(pair => pair.number).Min(), numberValuePairs.Select(pair => pair.number).Max(), @default)
        {
            Values = numberValuePairs.ToReadOnlyList(pair => pair.value);
            RawNumberByName = numberValuePairs.ToDictionary(pair => pair.value, pair => pair.number, StringComparer.Ordinal);
            NameByRawNumber = numberValuePairs.ToDictionary(pair => pair.number, pair => pair.value);
        }

        internal override FieldBase WithParent(FieldContainer parent) => new EnumField(parent, this);
    }
}
