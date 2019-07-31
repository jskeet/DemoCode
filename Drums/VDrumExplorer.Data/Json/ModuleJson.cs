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
        public int? Kits { get; set; }
        public int? InstrumentsPerKit { get; set; }

        public int? Triggers { get; set; }
        public List<InstrumentGroupJson>? InstrumentGroups { get; set; }
        public List<ContainerJson>? Containers { get; set; }
        public VisualTreeNodeJson? VisualTree { get; set; }

        internal static ModuleJson FromJson(JObject json)
        {
            var serializer = new JsonSerializer { Converters = { new HexInt32Converter() }, MissingMemberHandling = MissingMemberHandling.Error };
            return json.ToObject<ModuleJson>(serializer);
        }

        internal ContainerJson FindContainer(FieldPath path, string name) =>
            Containers.FirstOrDefault(c => c.Name == name) ?? throw new ModuleSchemaException(path, $"Unable to find container with name '{name}'");

        internal int? GetRepeat(string? repeat) =>
            repeat switch
            {
                null => (int?)null,
                "$kits" => Kits ?? throw new ModuleSchemaException($"Repeat value {repeat} invalid when {nameof(Kits)} is not specified"),
                "$instruments" => InstrumentsPerKit ?? throw new ModuleSchemaException($"Repeat value {repeat} invalid when {nameof(InstrumentsPerKit)} is not specified"),
                "$triggers" => Triggers ?? throw new ModuleSchemaException($"Repeat value {repeat} invalid when {nameof(Triggers)} is not specified"),
                _ => int.TryParse(repeat, NumberStyles.None, CultureInfo.InvariantCulture, out var result)
                    ? result
                    : throw new InvalidOperationException($"Invalid repeat value: '{repeat}'")
            };

        internal ModuleSchema ToModuleSchema()
        {
            var root = FieldPath.Root();
            var name = ValidateNotNull(root, Name, nameof(Name));
            var modelId = ValidateNotNull(root, ModelId, nameof(ModelId));
            var familyCode = ValidateNotNull(root, FamilyCode, nameof(FamilyCode));
            var familyNumberCode = ValidateNotNull(root, FamilyNumberCode, nameof(FamilyNumberCode));

            var rootContainer = FindContainer(root, "Root").ToContainer(this, root, new ModuleAddress(0), "Root", condition: null);
            var instrumentGroups = InstrumentGroups
                .Select((igj, index) => igj.ToInstrumentGroup(index))
                .ToList()
                .AsReadOnly();

            var fieldsByPath = rootContainer.DescendantsAndSelf().ToDictionary(f => f.Path).AsReadOnly();
            var context = VisualTreeConversionContext.Create(this, fieldsByPath);
            var visualRoot = ValidateNotNull(root, VisualTree, nameof(VisualTree))
                .ConvertVisualNodes(context).Single();

            return new ModuleSchema(name, modelId.Value, familyCode.Value, familyNumberCode.Value, rootContainer, instrumentGroups, visualRoot);
        }
    }
}
