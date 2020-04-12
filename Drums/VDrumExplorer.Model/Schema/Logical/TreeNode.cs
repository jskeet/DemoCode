// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Logical
{
    public class TreeNode
    {
        /// <summary>
        /// The name of this tree node; unique within its parent.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The absolute path to this tree node within the module.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The logical "root" container of this tree node.
        /// </summary>
        public IContainer Container;

        /// <summary>
        /// The child nodes in the tree.
        /// </summary>
        public IReadOnlyList<TreeNode> Children { get; }

        /// <summary>
        /// The detail groups of this tree node.
        /// </summary>
        public IReadOnlyList<INodeDetail> Details { get; }

        /// <summary>
        /// The formattable description of the node.
        /// </summary>
        public FieldFormattableString Format { get; }

        /// <summary>
        /// Parent node, or null for the root node.
        /// </summary>
        public TreeNode? Parent { get; internal set; }

        internal TreeNode Root => Parent?.Root ?? this;

        internal TreeNode(string name, string path, IContainer container, FieldFormattableString format, IReadOnlyList<TreeNode> children, IReadOnlyList<INodeDetail> details)
        {
            (Name, Path, Container, Format, Children, Details) = (name, path, container, format, children, details);
            foreach (TreeNode node in Children)
            {
                node.Parent = this;
            }
        }

        /// <summary>
        /// Resolves a tree node by path.
        /// </summary>
        public TreeNode ResolveNode(string path)
        {
            if (path == "." || path == "")
            {
                return this;
            }
            if (path.StartsWith("../"))
            {
                return Parent?.ResolveNode(path.Substring(3)) ?? throw new ArgumentException("Can't use .. in path from root node");
            }
            if (path.StartsWith("/"))
            {
                return Root.ResolveNode(path.Substring(1));
            }
            var segments = path.Split('/');
            TreeNode current = this;
            foreach (var segment in segments)
            {
                current = current.Children.FirstOrDefault(c => c.Name == segment)
                    ?? throw new ArgumentException($"Container '{segment}' not found within container '{current.Path}'");
            }
            return current;
        }

        public override string ToString() => $"Name: {Name}; Format: {Format}; Path: '{Container.Path}'; Children: {Children.Count}; Details: {Details.Count}";
    }
}
