using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VDrumExplorer.Data.Fields;

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

        internal IEnumerable<VisualTreeNode> ConvertVisualNodes(
            ModuleJson moduleJson, IReadOnlyDictionary<FieldPath, IField> fieldsByPath,
            FieldPath parentPath, IDictionary<string, string> indexes)
        {
            int? repeat = moduleJson.GetRepeat(Repeat);
            var relativePath = ValidateNotNull(parentPath, Path, nameof(Path));
            var unprocessedPath = parentPath + relativePath;
            if (repeat == null)
            {
                var description = BuildFormatElement(fieldsByPath, parentPath, indexes);
                FieldPath nodePath = parentPath + ReplaceIndexes(relativePath, indexes);
                yield return ToVisualTreeNode(nodePath, indexes, description);
            }
            else
            {
                var index = ValidateNotNull(unprocessedPath, Index, nameof(Index));
                for (int i = 1; i <= repeat; i++)
                {
                    Dictionary<string, string> newIndexes = new Dictionary<string, string>(indexes) { { index, i.ToString(CultureInfo.InvariantCulture) } };
                    FieldPath nodePath = parentPath + ReplaceIndexes(relativePath, newIndexes);

                    yield return ToVisualTreeNode(nodePath, newIndexes, BuildFormatElement(fieldsByPath, parentPath, newIndexes));
                }
            }

            VisualTreeNode ToVisualTreeNode(FieldPath nodePath, IDictionary<string, string> indexes, VisualTreeDetail.FormatElement description)
            {
                var children = Children.SelectMany(child => child.ConvertVisualNodes(moduleJson, fieldsByPath, nodePath, indexes)).ToList().AsReadOnly();
                var details = Details.Select(detail => detail.ToVisualTreeDetail(moduleJson, fieldsByPath, nodePath, indexes)).ToList().AsReadOnly();
                return new VisualTreeNode(children, details, description);
            }
        }

        private VisualTreeDetail.FormatElement BuildFormatElement(IReadOnlyDictionary<FieldPath, IField> fieldsByPath, FieldPath parentPath, IDictionary<string, string> indexes)
        {
            if (Description != null)
            {
                ValidateNull(parentPath, Format, nameof(Format), nameof(Description));
                ValidateNull(parentPath, FormatPaths, nameof(FormatPaths), nameof(Description));
                return new VisualTreeDetail.FormatElement(Description, null);
            }
            else
            {
                var format = ValidateNotNull(parentPath, Format, nameof(Format));
                var formatPaths = ValidateNotNull(parentPath, FormatPaths, nameof(FormatPaths));
                return BuildFormatElement(fieldsByPath, format, parentPath, formatPaths, indexes);
            }
        }

        internal static VisualTreeDetail.FormatElement BuildFormatElement(IReadOnlyDictionary<FieldPath, IField> fieldsByPath, string formatString, FieldPath parentPath, IEnumerable<string> formatPaths, IDictionary<string, string> indexes)
        {
            formatString = ReplaceIndexes(formatString, indexes);
            var formatFields = formatPaths
                .Select(p => parentPath + ReplaceIndexes(p, indexes))
                .Select(p => (IPrimitiveField) fieldsByPath[p])
                .ToList()
                .AsReadOnly();
            return new VisualTreeDetail.FormatElement(formatString, formatFields);
        }

        internal static string ReplaceIndexes(string text, IDictionary<string, string> indexes)
        {
            foreach (var pair in indexes)
            {
                text = text.Replace("$" + pair.Key, pair.Value);
            }
            return text;
        }
    }
}
