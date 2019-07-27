﻿using System.Collections.Generic;
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
        public string FormatString { get; }

        public FormattableDescription(string formatString, IReadOnlyList<IPrimitiveField>? formatFields) =>
            (FormatString, FormatFields) = (formatString, formatFields);

        public string Format(ModuleData data) =>
            FormatFields is null
            ? FormatString
            : string.Format(FormatString, FormatFields.Select(f => f.GetText(data)).ToArray());
    }
}