// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Layout
{
    /// <summary>
    /// A node in the visual tree. This has (potentially) child nodes,
    /// as well as details to display when this node is selected.
    /// </summary>
    public sealed class VisualTreeNode
    {
        /// <summary>
        /// The context in which fields should be resolved.
        /// </summary>
        public FixedContainer Context { get; }

        /// <summary>
        /// The parent of this node, or null for the root node.
        /// </summary>
        public VisualTreeNode? Parent { get; }

        public IReadOnlyList<VisualTreeNode> Children { get; }
        public IReadOnlyList<VisualTreeDetail> Details { get; }

        public FormattableDescription Description { get; }
        public FieldChain<MidiNoteField>? MidiNoteField { get; }

        /// <summary>
        /// The kit number for which this is the root node, if any.
        /// </summary>
        public int? KitNumber { get; }

        internal VisualTreeNode(
            VisualTreeNode? parent,
            FixedContainer context,
            Func<VisualTreeNode?, IReadOnlyList<VisualTreeNode>> childrenProvider,
            IReadOnlyList<VisualTreeDetail> details,
            FormattableDescription description,
            FieldChain<MidiNoteField>? midiNoteField,
            int? kitNumber) =>
            (Parent, Context, Children, Details, Description, MidiNoteField, KitNumber) =
            (parent, context, childrenProvider(this), details, description, midiNoteField, kitNumber);

        internal static VisualTreeNode FromFixedContainer(VisualTreeNode? parent, FixedContainer context)
        {
            var container = context.Container;
            Func<VisualTreeNode?, IReadOnlyList<VisualTreeNode>> childrenProvider = newNode =>
                container.Fields.OfType<Container>().Select(c => FromFixedContainer(newNode, new FixedContainer(c, context.Address + c.Offset))).ToList().AsReadOnly();
            var details = new List<VisualTreeDetail> { new VisualTreeDetail(container.Description, FieldChain<Container?>.EmptyChain(container)) }.AsReadOnly();
            return new VisualTreeNode(parent, context, childrenProvider, details, new FormattableDescription(container.Description, null), null, null);
        }

        public IEnumerable<VisualTreeNode> DescendantNodesAndSelf()
        {
            Queue<VisualTreeNode> nodeQueue = new Queue<VisualTreeNode>();
            nodeQueue.Enqueue(this);
            while (nodeQueue.Count != 0)
            {
                var node = nodeQueue.Dequeue();
                yield return node;
                foreach (var child in node.Children)
                {
                    nodeQueue.Enqueue(child);
                }
            }
        }
    }
}
