// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Layout;

namespace VDrumExplorer.Data.Json
{
    using static Validation;

    /// <summary>
    /// Keeps track of the context while converting <see cref="VisualTreeNodeJson"/>
    /// and <see cref="VisualTreeDetailJson"/> elements into their non-JSON equivalents.
    /// </summary>
    internal class VisualTreeConversionContext
    {
        private readonly ModuleJson moduleJson;
        private readonly IReadOnlyDictionary<FieldPath, IField> fieldsByPath;
        internal FieldPath Path { get; }
        private readonly IDictionary<string, string> indexes;

        private VisualTreeConversionContext(
            ModuleJson moduleJson, IReadOnlyDictionary<FieldPath, IField> fieldsByPath,
            FieldPath currentPath, IDictionary<string, string> indexes)
        {
            this.moduleJson = moduleJson;
            this.fieldsByPath = fieldsByPath;
            Path = currentPath;
            this.indexes = indexes;
        }

        internal static VisualTreeConversionContext Create(ModuleJson moduleJson, IReadOnlyDictionary<FieldPath, IField> fieldsByPath) =>
            new VisualTreeConversionContext(moduleJson, fieldsByPath, FieldPath.Root(), new Dictionary<string, string>());

        internal VisualTreeConversionContext WithIndex(string indexName, int indexValue)
        {
            var newIndexes = new Dictionary<string, string>(indexes)
            {
                { indexName, indexValue.ToString(CultureInfo.InvariantCulture) }
            };
            return new VisualTreeConversionContext(moduleJson, fieldsByPath, Path, newIndexes);
        }

        internal Container GetContainer(string relativePath)
        {
            FieldPath containerPath = Path + ReplaceIndexes(relativePath);
            Validate(Path, fieldsByPath.TryGetValue(containerPath, out var field), "Container not found");
            var container = field as Container;
            Validate(container is object, "Field is not a container");
            return container!;
        }

        private FieldPath GetPath(string relativePath) => Path + ReplaceIndexes(relativePath);

        internal VisualTreeConversionContext WithPath(string relativePath) =>
            new VisualTreeConversionContext(moduleJson, fieldsByPath, GetPath(relativePath), indexes);

        internal int? GetRepeat(string? repeat) => moduleJson.GetRepeat(repeat);

        internal FormattableDescription BuildDescription(string formatString, IEnumerable<string> formatPaths)
        {
            formatString = ReplaceIndexes(formatString);
            var formatFields = formatPaths
                .Select(GetPath)
                .Select(p => (IPrimitiveField) fieldsByPath[p])
                .ToList()
                .AsReadOnly();
            return new FormattableDescription(formatString, formatFields);
        }

        private string ReplaceIndexes(string text)
        {
            foreach (var pair in indexes)
            {
                text = text.Replace("$" + pair.Key, pair.Value);
            }
            return text;
        }
    }
}
