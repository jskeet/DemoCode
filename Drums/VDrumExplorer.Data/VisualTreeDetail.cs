using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data
{
    /// <summary>
    /// An element to display when a <see cref="VisualTreeNode"/> is selected.
    /// </summary>
    public sealed class VisualTreeDetail
    {
        public string Description { get; }
        
        // Either Container or FormatElements is set.
        public Container Container { get; }
        public IReadOnlyList<FormatElement> FormatElements { get; }

        public VisualTreeDetail(string description, Container container, IReadOnlyList<FormatElement> formatElements) =>
            (Description, Container, FormatElements) = (description, container, formatElements);

        // TODO: Move/rename this
        public sealed class FormatElement
        {
            public IReadOnlyList<IField> FormatFields { get; }
            public string FormatString { get; }

            public FormatElement(string formatString, IReadOnlyList<IField> formatFields) =>
                (FormatString, FormatFields) = (formatString, formatFields);

            public string Format(ModuleData data)
            {
                string[] formatArgs = FormatFields.Cast<IPrimitiveField>().Select(f => f.GetText(data)).ToArray();
                return string.Format(FormatString, formatArgs);
            }
        }
    }
}
