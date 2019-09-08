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

        public IReadOnlyList<VisualTreeNode> Children { get; }
        public IReadOnlyList<VisualTreeDetail> Details { get; }

        public FormattableDescription Description { get; }
        public FieldChain<MidiNoteField>? MidiNoteField { get; }

        public VisualTreeNode(
            FixedContainer context,
            IReadOnlyList<VisualTreeNode> children,
            IReadOnlyList<VisualTreeDetail> details,
            FormattableDescription description,
            FieldChain<MidiNoteField>? midiNoteField) =>
            (Context, Children, Details, Description, MidiNoteField) = (context, children, details, description, midiNoteField);

        internal static VisualTreeNode FromFixedContainer(FixedContainer context)
        {
            var container = context.Container;
            var children = container.Fields.OfType<Container>().Select(c => FromFixedContainer(new FixedContainer(c, context.Address + c.Offset))).ToList().AsReadOnly();
            var details = new List<VisualTreeDetail> { new VisualTreeDetail(container.Description, FieldChain<Container>.EmptyChain(container)) }.AsReadOnly();
            return new VisualTreeNode(context, children, details, new FormattableDescription(container.Description, null), null);
        }
    }
}
