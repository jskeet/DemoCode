using System.Collections.Generic;
using System.Globalization;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Json
{
    using static Validation;

    internal sealed class VisualTreeDetailJson
    {
        public string? Description { get; set; }
        public string? Path { get; set; }
        
        public List<string>? FormatPaths { get; set; }
        public string? Repeat { get; set; }
        public string? Format { get; set; }
        public string? Index { get; set; }


        internal VisualTreeDetail ToVisualTreeDetail(ModuleJson moduleJson, IReadOnlyDictionary<FieldPath, IField> fieldsByPath, FieldPath parentPath, IDictionary<string, string> indexes)
        {
            var description = ValidateNotNull(parentPath + "???", Description, nameof(Description));
            
            int? repeat = moduleJson.GetRepeat(Repeat);
            if (repeat == null)
            {
                string relativePath = ValidateNotNull(parentPath, Path, nameof(Path));
                var rawPath = parentPath + relativePath;
                ValidateNull(rawPath, Format, nameof(Format), nameof(Repeat));
                ValidateNull(rawPath, FormatPaths, nameof(FormatPaths), nameof(Repeat));

                FieldPath containerPath = parentPath + VisualTreeNodeJson.ReplaceIndexes(relativePath, indexes);
                Validate(rawPath, fieldsByPath.TryGetValue(containerPath, out var field), "Container not found");
                var container = field as Container;
                Validate(container is object, "Field is not a container");
                return new VisualTreeDetail(description, container!);
            }
            else
            {                
                var format = ValidateNotNull(parentPath, Format, nameof(Format));
                var formatPaths = ValidateNotNull(parentPath, FormatPaths, nameof(FormatPaths));
                var index = ValidateNotNull(parentPath, Index, nameof(Index));
                ValidateNull(parentPath + "???", Path, nameof(Path), nameof(Repeat));
                List<VisualTreeDetail.FormatElement> formatElements = new List<VisualTreeDetail.FormatElement>();
                for (int i = 1; i <= repeat; i++)
                {
                    Dictionary<string, string> newIndexes = new Dictionary<string, string>(indexes) { { index, i.ToString(CultureInfo.InvariantCulture) } };
                    var formatElement = VisualTreeNodeJson.BuildFormatElement(fieldsByPath, format, parentPath, formatPaths, newIndexes);
                    formatElements.Add(formatElement);
                }
                return new VisualTreeDetail(description, formatElements);
            }
        }

    }
}
