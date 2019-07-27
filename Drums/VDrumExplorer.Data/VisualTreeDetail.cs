using System.Collections.Generic;
using System.Linq;
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
        public Container? Container { get; }
        public IReadOnlyList<FormatElement>? FormatElements { get; }

        public VisualTreeDetail(string description, Container container) =>
            (Description, Container, FormatElements) = (description, container, null);

        public VisualTreeDetail(string description, IReadOnlyList<FormatElement> formatElements) =>
            (Description, Container, FormatElements) = (description, null, formatElements);

        // TODO: Move/rename this
        public sealed class FormatElement
        {
            public IReadOnlyList<IPrimitiveField>? FormatFields { get; }
            public string FormatString { get; }

            public FormatElement(string formatString, IReadOnlyList<IPrimitiveField>? formatFields) =>
                (FormatString, FormatFields) = (formatString, formatFields);

            public string Format(ModuleData data)
            {
                if (FormatFields == null)
                {
                    return FormatString;
                }
                string[] formatArgs = FormatFields.Select(f => f.GetText(data)).ToArray();
                return string.Format(FormatString, formatArgs);
            }
        }
    }
}
