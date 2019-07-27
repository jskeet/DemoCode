using System;
using System.Collections.Generic;
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

        public VisualTreeNode(
            IReadOnlyList<VisualTreeNode> children,
            IReadOnlyList<VisualTreeDetail> details,
            FormattableDescription description) =>
            (Children, Details, Description) = (children, details, description);
    }
}
