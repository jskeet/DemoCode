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
        public IReadOnlyList<VisualTreeNode> Children { get; }
        public IReadOnlyList<VisualTreeDetail> Details { get; }

        public FormattableDescription Description { get; }
        public MidiNoteField? MidiNoteField { get; }

        public VisualTreeNode(
            IReadOnlyList<VisualTreeNode> children,
            IReadOnlyList<VisualTreeDetail> details,
            FormattableDescription description,
            MidiNoteField? midiNoteField) =>
            (Children, Details, Description, MidiNoteField) = (children, details, description, midiNoteField);

        internal static VisualTreeNode FromContainer(Container container)
        {
            var children = container.Fields.OfType<Container>().Select(FromContainer).ToList().AsReadOnly();
            var details = new List<VisualTreeDetail> { new VisualTreeDetail(container.Description, container) }.AsReadOnly();
            return new VisualTreeNode(children, details, new FormattableDescription(container.Description, null), null);
        }
    }
}
