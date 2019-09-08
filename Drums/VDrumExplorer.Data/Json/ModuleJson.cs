// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Layout;
using static System.FormattableString;

namespace VDrumExplorer.Data.Json
{
    using static Validation;

    /// <summary>
    /// JSON representation of module data. This is the root document, effectively.
    /// </summary>
    internal sealed class ModuleJson
    {
        /// <summary>
        /// Developer-oriented comment. Has no effect.
        /// </summary>
        public string? Comment { get; set; }

        public string? Name { get; set; }
        public HexInt32? ModelId { get; set; }
        public HexInt32? FamilyCode { get; set; }
        public HexInt32? FamilyNumberCode { get; set; }
        public int? UserSamples { get; set; }
        public List<InstrumentGroupJson>? InstrumentGroups { get; set; }
        public Dictionary<string, ContainerJson>? Containers { get; set; }
        public VisualTreeNodeJson? LogicalTree { get; set; }
        public Dictionary<string, int>? Counts { get; set; }
        
        public List<LookupJson>? Lookups { get; set; }

        internal static ModuleJson FromJson(JObject json)
        {
            var serializer = new JsonSerializer { Converters = { new HexInt32Converter() }, MissingMemberHandling = MissingMemberHandling.Error };
            return json.ToObject<ModuleJson>(serializer);
        }

        internal int? GetCount(string? repeat) =>
            repeat switch
            {
                null => (int?) null,
                _ when repeat.StartsWith("$") && Counts != null && Counts.TryGetValue(repeat.Substring(1), out var count) => count,
                _ when int.TryParse(repeat, NumberStyles.None, CultureInfo.InvariantCulture, out var result) => result,
                _ => throw new InvalidOperationException($"Invalid count value: '{repeat}'")
            };

        internal void Validate()
        {
            ValidateNotNull(Name, nameof(Name));
            ValidateNotNull(ModelId, nameof(ModelId));
            ValidateNotNull(FamilyCode, nameof(FamilyCode));
            ValidateNotNull(FamilyNumberCode, nameof(FamilyNumberCode));
            ValidateNotNull(LogicalTree, nameof(LogicalTree));
            ValidateNotNull(UserSamples, nameof(UserSamples));
            ValidateNotNull(Containers, nameof(Containers));
            Lookups?.ForEach(lookup => lookup.Validate(GetCount));
            Validation.Validate(Containers.TryGetValue("Root", out _), "No root container present");
        }

        internal Container BuildRootContainer(ModuleSchema schema)
        {
            return Containers!["Root"].ToContainer(schema, this, "Root", 0, "Root", condition: null);
        }
        
        internal IReadOnlyList<InstrumentGroup> BuildInstrumentGroups() =>
            InstrumentGroups
                .Select((igj, index) => igj.ToInstrumentGroup(index))
                .ToList()
                .AsReadOnly();

        internal VisualTreeNode BuildLogicalRoot(FixedContainer root)
        {
            // This is ugly to do with LINQ...
            var lookupsByPath = new Dictionary<string, string>();
            foreach (var lookup in Lookups ?? Enumerable.Empty<LookupJson>())
            {
                for (int i = 0; i < lookup.Values!.Count; i++)
                {
                    var path = Invariant($"/lookups/{lookup.Name}[{i + 1}]");
                    lookupsByPath[path] = lookup.Values[i];
                }
            }
            var context = VisualTreeConversionContext.Create(this, root, lookupsByPath.AsReadOnly());
            return LogicalTree!.ConvertVisualNodes(context).Single();
        }
    }
}
