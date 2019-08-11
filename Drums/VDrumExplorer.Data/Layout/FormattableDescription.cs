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
        public IReadOnlyList<IPrimitiveField>? FormatFields { get; }
        public IEnumerable<IPrimitiveField> FormatFieldsOrEmpty => FormatFields ?? Enumerable.Empty<IPrimitiveField>();
        public string FormatString { get; }

        public FormattableDescription(string formatString, IReadOnlyList<IPrimitiveField>? formatFields) =>
            (FormatString, FormatFields) = (formatString, formatFields);

        public string Format(ModuleData data) =>
            FormatFields is null
            ? FormatString
            : string.Format(FormatString, FormatFields.Select(f => f.GetText(data)).ToArray());
    }
}