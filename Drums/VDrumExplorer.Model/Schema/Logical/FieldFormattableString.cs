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
        private static readonly IReadOnlyList<string> EmptyFormatPaths = new string[0];

        /// <summary>
        /// The format string, suitable for use with <see cref="string.Format(string, object[])"/>.
        /// </summary>
        public string FormatString { get; }

        /// <summary>
        /// The container that the format paths should be resolved relative to.
        /// </summary>
        public IContainer Container { get; }

        /// <summary>
        /// The paths (relative or absolute) of the fields being formatted. This is never null, but may be empty.
        /// </summary>
        public IReadOnlyList<string> FormatPaths { get; }

        private FieldFormattableString(IContainer container, string formatString, IReadOnlyList<string>? formatPaths) =>
            (Container, FormatString, FormatPaths) = (container, formatString, formatPaths ?? EmptyFormatPaths);

        internal static FieldFormattableString Create(IContainer container, string formatString, IEnumerable<string>? formatPaths, SchemaVariables variables) =>
            new FieldFormattableString(container, variables.Replace(formatString), formatPaths?.ToReadOnlyList(formatPath => variables.Replace(formatPath)));

        public override string ToString() => FormatString;
    }
}
