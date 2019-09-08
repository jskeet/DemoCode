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
        private readonly IReadOnlyDictionary<string, string> lookupsByPath;
        internal FixedContainer ContainerContext { get; }
        private readonly IDictionary<string, string> indexes;

        private VisualTreeConversionContext(
            ModuleJson moduleJson,
            FixedContainer containerContext,
            IReadOnlyDictionary<string, string> lookupsByPath,
            IDictionary<string, string> indexes)
        {
            this.moduleJson = moduleJson;
            this.ContainerContext = containerContext;
            this.lookupsByPath = lookupsByPath;
            this.indexes = indexes;
        }

        internal static VisualTreeConversionContext Create(
            ModuleJson moduleJson,
            FixedContainer containerContext,
            IReadOnlyDictionary<string, string> lookupsByPath) =>
            new VisualTreeConversionContext(moduleJson, containerContext, lookupsByPath, new Dictionary<string, string>());

        internal VisualTreeConversionContext WithIndex(string indexName, int indexValue)
        {
            var newIndexes = new Dictionary<string, string>(indexes)
            {
                { indexName, indexValue.ToString(CultureInfo.InvariantCulture) }
            };
            return new VisualTreeConversionContext(moduleJson, ContainerContext, lookupsByPath, newIndexes);
        }

        internal FieldChain<Container> GetContainer(string relativePath) =>
            relativePath == "."
            ? FieldChain<Container>.EmptyChain(ContainerContext.Container)
            : FieldChain<Container>.Create(ContainerContext.Container, ReplaceIndexes(relativePath));

        internal FieldChain<MidiNoteField> GetMidiNoteField(string relativePath) =>
            FieldChain<MidiNoteField>.Create(ContainerContext.Container, ReplaceIndexes(relativePath));

        internal VisualTreeConversionContext WithPath(string relativePath)
        {
            if (relativePath == ".")
            {
                return this;
            }
            var chain = FieldChain<Container>.Create(ContainerContext.Container, ReplaceIndexes(relativePath));
            var newContainerContext = chain.GetFinalContext(ContainerContext).ToChildContext(chain.FinalField);
            return new VisualTreeConversionContext(moduleJson, newContainerContext, lookupsByPath, indexes);
        }

        internal int? GetRepeat(string? repeat) => moduleJson.GetCount(repeat);

        internal FormattableDescription BuildDescription(string formatString, IEnumerable<string> formatPaths)
        {
            formatString = ReplaceIndexes(formatString);
            var formatFields = formatPaths
                .Select(GetFormattableString)
                .ToList()
                .AsReadOnly();
            return new FormattableDescription(formatString, formatFields);
        }

        private IModuleDataFormattableString GetFormattableString(string path)
        {
            var indexed = ReplaceIndexes(path);
            if (lookupsByPath.TryGetValue(indexed, out var lookupValue))
            {
                return new LookupFormattableString(indexed, lookupValue);
            }
            var fieldChain = FieldChain<IPrimitiveField>.Create(ContainerContext.Container, indexed);
            return new FieldFormattableString(fieldChain);
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
