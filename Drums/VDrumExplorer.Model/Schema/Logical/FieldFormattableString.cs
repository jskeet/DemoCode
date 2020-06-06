// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Logical
{
    /// <summary>
    /// Represents a string that can be formatted using the data within fields.
    /// </summary>
    public sealed class FieldFormattableString
    {
        /// <summary>
        /// The format string, suitable for use with <see cref="string.Format(string, object[])"/>.
        /// </summary>
        public string FormatString { get; }

        /// <summary>
        /// The absolute paths of the fields being formatted. This is never null, but may be empty.
        /// </summary>
        public IReadOnlyList<string> FieldPaths { get; }

        internal IReadOnlyList<IField>? Fields { get; }

        private FieldFormattableString(string formatString, IReadOnlyList<IField>? fields) =>
            (FormatString, Fields, FieldPaths) =
            (formatString, fields, (fields?.Select(field => field.Path) ?? Enumerable.Empty<string>()).ToReadOnlyList());

        internal static FieldFormattableString Create(IContainer container, string formatString, IEnumerable<string>? formatPaths, SchemaVariables variables) =>
            new FieldFormattableString(variables.Replace(formatString),
                formatPaths?
                    .Select(formatPath => container.ResolveField(variables.Replace(formatPath)))
                    .ToList()
                    .AsReadOnly());

        public override string ToString() => FormatString;
    }
}
