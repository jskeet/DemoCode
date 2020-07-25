// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

#nullable disable warnings

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Json
{
    internal sealed class LogicalTreeDetailJson
    {
        /// <summary>
        /// The description of the overall set of fields.
        /// </summary>
        public string? Description { get; set; }


        /// <summary>
        /// The path relative to the parent of the field container with the fields to display.
        /// </summary>
        public string? Path { get; set; }
        
        /// <summary>
        /// The paths of values to obtain from fields, relative to the current node.
        /// These may refer to {index} and {item} obtained from the Repeat.
        /// </summary>
        public List<string>? FormatPaths { get; set; }

        /// <summary>
        /// The string used to format the value, where each of {0}, {1} etc
        /// is replaced by the value obtained from the corresponding format path.
        /// The format string will have {index} and {item} values replaced
        /// from the Repeat before regular formatting is performed.
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// The repetition count/lookup of the formatted value.
        /// </summary>
        public string? Repeat { get; set; }

        internal INodeDetail ToNodeDetail(ModuleJson module, IContainer nodeContainer, SchemaVariables nodeVariables)
        {
            Validation.Validate(Path is null ^ FormatPaths is null,
                "Exactly one of Path and FormatPaths must be specified");

            if (Path is object)
            {
                var container = nodeContainer.ResolveContainer(nodeVariables.Replace(Path));
                return container is FieldContainer fc
                    ? new FieldContainerNodeDetail(Description, fc)
                    : throw new ArgumentException($"'{Path}' does not resolve to a field container");
            }
            else
            {
                var formattableFields = module.GetRepeatSequence(Repeat, SchemaVariables.Empty)
                    .ToReadOnlyList(tuple => FieldFormattableString.Create(nodeContainer, Format, FormatPaths, tuple.variables));
                return new ListNodeDetail(Description, formattableFields);
            }
        }
    }
}
