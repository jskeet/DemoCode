// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
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
        private readonly IReadOnlyDictionary<FieldPath, string> lookupsByPath;
        internal FieldPath Path { get; }
        private readonly IDictionary<string, string> indexes;

        private VisualTreeConversionContext(
            ModuleJson moduleJson,
            IReadOnlyDictionary<FieldPath, IField> fieldsByPath,
            IReadOnlyDictionary<FieldPath, string> lookupsByPath,
            FieldPath currentPath, IDictionary<string, string> indexes)
        {
            this.moduleJson = moduleJson;
            this.fieldsByPath = fieldsByPath;
            this.lookupsByPath = lookupsByPath;
            Path = currentPath;
            this.indexes = indexes;
        }

        internal static VisualTreeConversionContext Create(
            ModuleJson moduleJson,
            IReadOnlyDictionary<FieldPath, IField> fieldsByPath,
            IReadOnlyDictionary<FieldPath, string> lookupsByPath) =>
            new VisualTreeConversionContext(moduleJson, fieldsByPath, lookupsByPath, FieldPath.Root(), new Dictionary<string, string>());

        internal VisualTreeConversionContext WithIndex(string indexName, int indexValue)
        {
            var newIndexes = new Dictionary<string, string>(indexes)
            {
                { indexName, indexValue.ToString(CultureInfo.InvariantCulture) }
            };
            return new VisualTreeConversionContext(moduleJson, fieldsByPath, lookupsByPath, Path, newIndexes);
        }

        internal Container GetContainer(string relativePath)
        {
            FieldPath containerPath = Path + ReplaceIndexes(relativePath);
            Validate(containerPath, fieldsByPath.TryGetValue(containerPath, out var field), "Container not found");
            var container = field as Container;
            Validate(container is object, "Field is not a container");
            return container!;
        }

        internal MidiNoteField GetMidiNoteField(string relativePath)
        {
            FieldPath fieldPath = Path + ReplaceIndexes(relativePath);
            Validate(Path, fieldsByPath.TryGetValue(fieldPath, out var field), "Container not found");
            var primitive = field as MidiNoteField;
            Validate(primitive is object, "Field is not a Midi note field");
            return primitive!;
        }

        private FieldPath GetPath(string relativePath)
        {
            var replaced = ReplaceIndexes(relativePath);
            return replaced.StartsWith("/") ? new FieldPath(replaced.Substring(1)) : Path + replaced;
        }

        internal VisualTreeConversionContext WithPath(string relativePath) =>
            new VisualTreeConversionContext(moduleJson, fieldsByPath, lookupsByPath, GetPath(relativePath), indexes);

        internal int? GetRepeat(string? repeat) => moduleJson.GetCount(repeat);

        internal FormattableDescription BuildDescription(string formatString, IEnumerable<string> formatPaths)
        {
            formatString = ReplaceIndexes(formatString);
            var formatFields = formatPaths
                .Select(GetPath)
                .Select(GetFormattableString)
                .ToList()
                .AsReadOnly();
            return new FormattableDescription(formatString, formatFields);
        }

        private IModuleDataFormattableString GetFormattableString(FieldPath path)
        {
            if (fieldsByPath.TryGetValue(path, out var field) && field is IPrimitiveField primitive)
            {
                return new FieldFormattableString(primitive);
            }
            if (lookupsByPath.TryGetValue(path, out var lookupValue))
            {
                return new LookupFormattableString(path, lookupValue);
            }
            throw new InvalidOperationException($"Path {path} not found as a primitive field or lookup");
        }

        private string ReplaceIndexes(string text)
        {
            // Replace longer variables first
            foreach (var pair in indexes.OrderByDescending(p => p.Key.Length))
            {
                text = text.Replace("$" + pair.Key, pair.Value);
            }
            return text;
        }
    }
}
