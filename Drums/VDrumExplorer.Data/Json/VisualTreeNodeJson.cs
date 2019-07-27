using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Data.Layout;

namespace VDrumExplorer.Data.Json
{
    using static Validation;

    internal sealed class VisualTreeNodeJson
    {
        public List<VisualTreeNodeJson> Children { get; set; } = new List<VisualTreeNodeJson>();
        public List<VisualTreeDetailJson> Details { get; set; } = new List<VisualTreeDetailJson>();
        public string? Description { get; set; }
        public string? Path { get; set; }

        public List<string>? FormatPaths { get; set; }
        public string? Repeat { get; set; }
        public string? Format { get; set; }
        public string? Index { get; set; }

        internal IEnumerable<VisualTreeNode> ConvertVisualNodes(VisualTreeConversionContext parentContext)
        {
            int? repeat = parentContext.GetRepeat(Repeat);
            var relativePath = ValidateNotNull(parentContext.Path, Path, nameof(Path));
            if (repeat == null)
            {
                var context = parentContext.WithPath(relativePath);
                yield return ToVisualTreeNode(context);
            }
            else
            {
                var index = ValidateNotNull(parentContext.Path, Index, nameof(Index));
                for (int i = 1; i <= repeat; i++)
                {
                    var context = parentContext.WithIndex(index, i).WithPath(relativePath);
                    yield return ToVisualTreeNode(context);
                }
            }

            VisualTreeNode ToVisualTreeNode(VisualTreeConversionContext context)
            {
                FormattableDescription description = BuildDescription(context);
            
                var children = Children.SelectMany(child => child.ConvertVisualNodes(context)).ToList().AsReadOnly();
                var details = Details.Select(detail => detail.ToVisualTreeDetail(context)).ToList().AsReadOnly();
                return new VisualTreeNode(children, details, description);
            }

            FormattableDescription BuildDescription(VisualTreeConversionContext context)
            {
                if (Description != null)
                {
                    ValidateNull(parentContext.Path, Format, nameof(Format), nameof(Description));
                    ValidateNull(parentContext.Path, FormatPaths, nameof(FormatPaths), nameof(Description));
                    return new FormattableDescription(Description, null);
                }
                else
                {
                    var format = ValidateNotNull(parentContext.Path, Format, nameof(Format));
                    var formatPaths = ValidateNotNull(parentContext.Path, FormatPaths, nameof(FormatPaths));
                    return context.BuildDescription(format, formatPaths);
                }
            }
        }
    }
}
