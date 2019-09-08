// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Layout
{
    /// <summary>
    /// A description that may derive some data from a field.
    /// This differs from <see cref="IPrimitiveField"/> in that it is self-contained (without a separate description),
    /// and it may refer to any number of fields (including zero).
    /// </summary>
    public sealed class FormattableDescription
    {
        internal IReadOnlyList<IModuleDataFormattableString>? FormatFields { get; }
        internal string FormatString { get; }

        internal FormattableDescription(string formatString, IReadOnlyList<IModuleDataFormattableString>? formatFields) =>
            (FormatString, FormatFields) = (formatString, formatFields);

        public string Format(FixedContainer context, ModuleData data) =>
            FormatFields is null
            ? FormatString
            : string.Format(FormatString, FormatFields.Select(f => f.Format(context, data)).ToArray());

        public IEnumerable<ModuleAddress> GetSegmentAddresses(FixedContainer context) =>
            FormatFields is null
            ? Enumerable.Empty<ModuleAddress>()
            : FormatFields.Select(f => f.GetSegmentAddress(context)).OfType<ModuleAddress>().Distinct();
    }
}