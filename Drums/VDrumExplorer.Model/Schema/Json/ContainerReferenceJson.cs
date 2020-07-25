// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

#nullable disable warnings

using System;
using System.Collections.Generic;
using VDrumExplorer.Model.Schema.Physical;
using static System.FormattableString;

namespace VDrumExplorer.Model.Schema.Json
{
    /// <summary>
    /// A reference to another container, within a "container container".
    /// </summary>
    internal class ContainerReferenceJson : ContainerItemBase
    {
        /// <summary>
        /// Type of container being referenced; defaults to Name if
        /// not set.
        /// </summary>
        public string? Container { get; set; }

        /// <summary>
        /// Any extra variables to include in this item.
        /// </summary>
        public Dictionary<string, string> ExtraVariables { get; set; }

        /// <summary>
        /// Offset within the parent container. (This is the offset of
        /// the first element if the item is repeated.)
        /// </summary>
        public HexInt32? Offset { get; set; }

        private ContainerJson? resolvedContainer;

        internal override void ValidateJson(ModuleJson module)
        {
            base.ValidateJson(module);
            // Offset must always be specified
            Validation.ValidateNotNull(Offset, nameof(Offset));
            if (Repeat is object)
            {
                Validation.ValidateNotNull(Repeat.Gap, $"{nameof(Repeat)}.{nameof(Repeat.Gap)}");
            }
            string containerName = Container ?? Name ?? throw new InvalidOperationException($"Either {nameof(Container)} or {nameof(Name)} must be specified");
            Validation.Validate(module.Containers.TryGetValue(containerName, out resolvedContainer),
                "Container '{0}' not found.", containerName);
        }

        internal IEnumerable<IContainer> ToContainers(ModuleSchema schema, ModuleJson module, ModuleAddress parentAddress, string? parentPath, SchemaVariables variables)
        {
            variables = variables.WithVariables(ExtraVariables);
            ModuleOffset offset = ModuleOffset.FromDisplayValue(Offset.Value);
            if (Repeat is null)
            {
                yield return resolvedContainer.ToContainer(schema, module, ResolvedName, ResolvedDescription,
                    parentAddress + offset, parentPath, variables);
            }
            else
            {
                int gap = ModuleOffset.FromDisplayValue(Repeat.Gap.Value).LogicalValue;
                foreach (var tuple in module.GetRepeatSequence(Repeat.Items, variables))
                {
                    var itemVariables = variables.WithVariable(Repeat.IndexVariable, tuple.index, Repeat.IndexTemplate);
                    var formattedDescription = tuple.variables.Replace(ResolvedDescription);
                    var formattedName = Invariant($"{ResolvedName}[{tuple.index}]");
                    yield return resolvedContainer.ToContainer(schema, module, formattedName, formattedDescription,
                        parentAddress + offset, parentPath, itemVariables);
                    offset += gap;
                }
            }
        }
    }
}
