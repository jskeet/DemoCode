// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;
using static System.FormattableString;

namespace VDrumExplorer.Model.Schema.Json
{
    internal sealed class LogicalTreeNodeJson
    {
        public List<LogicalTreeNodeJson> Children { get; set; } = new List<LogicalTreeNodeJson>();
        public List<LogicalTreeDetailJson> Details { get; set; } = new List<LogicalTreeDetailJson>();

        /// <summary>
        /// Name of this node.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Path of the container for this node relative to its parent.
        /// </summary>
        public string? Path { get; set; }
        public string? MidiNotePath { get; set; }

        /// <summary>
        /// The count or lookup to determine how many times the node is repeated.
        /// This introduces {index} and {item} entries into the variable collection.
        /// </summary>
        public string? Repeat { get; set; }

        /// <summary>
        /// Paths to fields to be resolved before formatting, resulting in {0}, {1} etc
        /// variables.
        /// </summary>
        public List<string>? FormatPaths { get; set; }

        /// <summary>
        /// The format string to apply in order to retrieve the value.
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// The format string to apply when only a single kit is being displayed.
        /// </summary>
        public string? KitOnlyFormat { get; set; }

        internal IEnumerable<TreeNode> ToTreeNodes(ModuleJson json, string? parentNodePath, IContainer parentContainer)
        {
            Preconditions.AssertNotNull(Name);
            Preconditions.AssertNotNull(Path);
            Preconditions.AssertNotNull(Format);
            if (Repeat is null)
            {
                yield return ToTreeNode(json, Name, parentContainer, SchemaVariables.Empty);
            }
            else
            {
                foreach (var tuple in json.GetRepeatSequence(Repeat, SchemaVariables.Empty))
                {
                    string name = Invariant($"{Name}[{tuple.index}]");
                    yield return ToTreeNode(json, name, parentContainer, tuple.variables);
                }
            }

            TreeNode ToTreeNode(ModuleJson json, string name, IContainer parentContainer, SchemaVariables variables)
            {
                string nodePath = PathUtilities.AppendPath(parentNodePath, name);
                var resolvedContainerPath = variables.Replace(Path!);
                var container = parentContainer.ResolveContainer(resolvedContainerPath);
                var formattableString = FieldFormattableString.Create(container, Format!, FormatPaths, variables);

                var childTreeNodes = new List<TreeNode>();
                foreach (var child in Children)
                {
                    childTreeNodes.AddRange(child.ToTreeNodes(json, nodePath, container));
                }
                var treeDetails = Details.Select(detail => detail.ToNodeDetail(json, container, variables)).ToList().AsReadOnly();
                return new TreeNode(name, nodePath, container, formattableString, childTreeNodes.AsReadOnly(), treeDetails);
            }
        }
    }
}
