// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Schema.Json
{
    using static Validation;

    /// <summary>
    /// JSON representation of module data. This is the root document, effectively.
    /// </summary>
    internal sealed class ModuleJson
    {
        /// <summary>
        /// Identifier for this module.
        /// </summary>
        public ModuleIdentifierJson? Identifier { get; set; }
        public List<InstrumentGroupJson>? InstrumentGroups { get; set; }
        public Dictionary<string, ContainerJson>? Containers { get; set; }
        public LogicalTreeNodeJson? LogicalTree { get; set; }

        public Dictionary<string, int>? Counts { get; set; }
        
        public Dictionary<string, List<string>>? Lookups { get; set; }

        public string? KitRootPathFormat { get; set; }
        public string? MainInstrumentPathFormat { get; set; }
        public string? KitNamePath { get; set; }
        public string? KitSubNamePath { get; set; }
        public string? TriggerPathFormat { get; set; }

        internal static ModuleJson FromJson(JObject json)
        {
            var serializer = new JsonSerializer { Converters = { new HexInt32Converter() }, MissingMemberHandling = MissingMemberHandling.Error };
            return json.ToObject<ModuleJson>(serializer) ?? throw new InvalidOperationException("JSON deserialization returned null");
        }

        internal void Validate()
        {
            ValidateNotNull(Identifier, nameof(Identifier));
            ValidateNotNull(LogicalTree, nameof(LogicalTree));
            ValidateNotNull(Counts, nameof(Counts));
            ValidateNotNull(Lookups, nameof(Lookups));
            ValidateNotNull(Containers, nameof(Containers));
            ValidateNotNull(KitRootPathFormat, nameof(Containers));
            ValidateNotNull(MainInstrumentPathFormat, nameof(MainInstrumentPathFormat));
            ValidateNotNull(KitNamePath, nameof(KitNamePath));
            ValidateNotNull(KitSubNamePath, nameof(KitSubNamePath));
            Validation.Validate(Containers.TryGetValue("Root", out _), "No root container present");

            foreach (var pair in Containers)
            {
                pair.Value.NameInModuleDictionary = pair.Key;
                pair.Value.ValidateAndResolve(this);
            }
        }

        internal TreeNode BuildLogicalRoot(ContainerContainer physicalRoot) =>
            LogicalTree!.ToTreeNodes(this, parentNodePath: null, parentContainer: physicalRoot).Single();

        internal ContainerContainer BuildPhysicalRoot(ModuleSchema schema) =>
            (ContainerContainer) Containers!["Root"].ToContainer(
                schema, this, name: "Root", description: "Root",
                ModuleAddress.FromLogicalValue(0), parentPath: null,
                SchemaVariables.Empty);

        internal IEnumerable<(string item, string index, SchemaVariables variables)> GetRepeatSequence(string items, SchemaVariables initialVariables) =>
            GetRepeatSequence(items)
                .Select((item, index) => (item, index: (index + 1).ToString(CultureInfo.InvariantCulture)))
                .Select(tuple => (tuple.item, tuple.index, initialVariables.WithVariable("item", tuple.item, "{item}").WithVariable("index", tuple.index, "{index}")));

        private Dictionary<string, List<string>> repeatSequenceCache = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        private IEnumerable<string> GetRepeatSequence(string items)
        {
            if (repeatSequenceCache.TryGetValue(items, out var result))
            {
                return result;
            }
            var uncached = GetRepeatSequenceUncached(items);
            var list = uncached as List<string> ?? uncached.ToList();
            repeatSequenceCache[items] = list;
            return list;
        }

        private IEnumerable<string> GetRepeatSequenceUncached(string items) =>
            Lookups!.TryGetValue(items, out var sequence) ? sequence
            : Counts!.TryGetValue(items, out var count) ? GenerateCountSequence(count)
            : int.TryParse(items, out count) ? GenerateCountSequence(count)
            : throw new ArgumentException($"Repeat value '{items}' is not a lookup, a count, or a number.");

        private static IEnumerable<string> GenerateCountSequence(int count) =>
            Enumerable.Range(1, count).Select(index => index.ToString(CultureInfo.InvariantCulture));

    }
}
