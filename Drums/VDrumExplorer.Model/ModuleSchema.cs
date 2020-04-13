// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDrumExplorer.Midi;
using VDrumExplorer.Model.Schema.Json;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model
{
    /// <summary>
    /// Schema for a module, containing the layout of the fields that
    /// can be populated by a <see cref="ModuleData"/>.
    /// </summary>
    public sealed class ModuleSchema
    {
        /// <summary>
        /// The known set of schemas, loaded lazily.
        /// </summary>
        public static IReadOnlyDictionary<ModuleIdentifier, Lazy<ModuleSchema>> KnownSchemas { get; }
            = new Dictionary<ModuleIdentifier, Lazy<ModuleSchema>>
            {
                //{ ModuleIdentifier.TD17, LazyFromAssemblyResources("TD17", "TD17.json") },
                { ModuleIdentifier.TD27, LazyFromAssemblyResources("TD27", "TD27.json") },
                //{ ModuleIdentifier.TD50, LazyFromAssemblyResources("TD50", "TD50.json") }
            }.AsReadOnly();

        /// <summary>
        /// The identifier for the module.
        /// </summary>
        public ModuleIdentifier Identifier { get; }

        public ContainerContainer PhysicalRoot { get; }

        public TreeNode LogicalRoot { get; }

        public IReadOnlyList<Instrument> PresetInstruments { get; }
        public IReadOnlyList<Instrument> UserSampleInstruments { get; }
        public IReadOnlyList<InstrumentGroup> InstrumentGroups { get; }
        public IReadOnlyList<TreeNode> KitRoots { get; }

        private ModuleSchema(ModuleJson json)
        {
            // Note: we populate everything other than the fields first, so that field
            // construction can rely on it.
            json.Validate();
            Identifier = json.Identifier!.ToModuleIdentifier();
            InstrumentGroups = json.InstrumentGroups
                .Select((igj, index) => igj.ToInstrumentGroup(index))
                .ToList()
                .AsReadOnly();

            PresetInstruments = InstrumentGroups.SelectMany(ig => ig.Instruments)
                .OrderBy(i => i.Id)
                .ToList()
                .AsReadOnly();
            // Just validate that our list is consistent.
            for (int i = 0; i < PresetInstruments.Count; i++)
            {
                if (PresetInstruments[i].Id != i)
                {
                    throw new InvalidOperationException($"Instrument {PresetInstruments[i]} is in index {i}");
                }
            }

            UserSampleInstruments = Enumerable.Range(0, json.Counts!["userSamples"])
                .Select(id => Instrument.FromUserSample(id))
                .ToList()
                .AsReadOnly();

            PhysicalRoot = json.BuildPhysicalRoot(this);
            LogicalRoot = json.BuildLogicalRoot(PhysicalRoot);

            // Note: this makes an assumption about the schema, but it appears to be reasonable.
            KitRoots = LogicalRoot.Children.Single(child => child.Name == "Kits").Children.ToReadOnlyList();
        }

        private static Lazy<ModuleSchema> LazyFromAssemblyResources(string resourceBase, string resourceName) =>
            new Lazy<ModuleSchema>(() => FromAssemblyResources(typeof(ModuleSchema).Assembly, resourceBase, resourceName));

        public static ModuleSchema FromAssemblyResources(Assembly assembly, string resourceBase, string resourceName) =>
            FromJson(JsonLoader.FromAssemblyResources(assembly, resourceBase).LoadResource(resourceName));

        public static ModuleSchema FromDirectory(string path, string resourceName) =>
            FromJson(JsonLoader.FromDirectory(path).LoadResource(resourceName));

        private static ModuleSchema FromJson(JObject json) =>
            new ModuleSchema(ModuleJson.FromJson(json));

    }
}