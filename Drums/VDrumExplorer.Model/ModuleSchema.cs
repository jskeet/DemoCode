// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDrumExplorer.Midi;
using VDrumExplorer.Model.Schema.Fields;
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
                { ModuleIdentifier.TD17, LazyFromAssemblyResources("TD17", "TD17.json") },
                { ModuleIdentifier.TD27, LazyFromAssemblyResources("TD27", "TD27.json") },
                { ModuleIdentifier.TD50, LazyFromAssemblyResources("TD50", "TD50.json") }
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

        /// <summary>
        /// The number of kits in this module.
        /// </summary>
        public int Kits { get; }

        /// <summary>
        /// The number of user samples in this module.
        /// </summary>
        public int UserSamples { get; }

        /// <summary>
        /// The logical root node for the first kit. This is often used for relocating kit data.
        /// </summary>
        public TreeNode Kit1Root => kitRoots[0];

        private readonly TreeNode[] kitRoots;
        private readonly string mainInstrumentPathFormat;
        private readonly string triggerPathFormat;
        internal string KitNamePath { get; }
        internal string KitSubNamePath { get; }

        private ModuleSchema(ModuleJson json)
        {
            // Note: we populate everything other than the fields first, so that field
            // construction can rely on it.
            json.Validate();
            Identifier = json.Identifier!.ToModuleIdentifier();
            Kits = json.Counts!["kits"];
            UserSamples = json.Counts!["userSamples"];
            InstrumentGroups = json.InstrumentGroups
                .Select((igj, index) => igj.ToInstrumentGroup(index))
                .Concat(new[] { InstrumentGroup.ForUserSamples(json.InstrumentGroups!.Count, UserSamples) })
                .ToReadOnlyList();

            PresetInstruments = InstrumentGroups
                .TakeWhile(ig => ig.Preset)
                .SelectMany(ig => ig.Instruments)
                .OrderBy(i => i.Id)
                .ToReadOnlyList();
            // Just validate that our list is consistent.
            for (int i = 0; i < PresetInstruments.Count; i++)
            {
                if (PresetInstruments[i].Id != i)
                {
                    throw new InvalidOperationException($"Instrument {PresetInstruments[i]} is in index {i}");
                }
            }

            UserSampleInstruments = InstrumentGroups.Last().Instruments;

            PhysicalRoot = json.BuildPhysicalRoot(this);
            LogicalRoot = json.BuildLogicalRoot(PhysicalRoot);
            LogicalRoot.ValidateFieldReferences();
            kitRoots = new TreeNode[Kits];
            for (int i = 1; i <= Kits; i++)
            {
                var root = LogicalRoot.ResolveNode(string.Format(json.KitRootPathFormat, i));
                kitRoots[i - 1] = root;
                root.KitNumber = i;
            }
            
            mainInstrumentPathFormat = json.MainInstrumentPathFormat!;
            KitNamePath = json.KitNamePath!;
            KitSubNamePath = json.KitSubNamePath!;
            triggerPathFormat = json.TriggerPathFormat!;
        }

        public TreeNode GetKitRoot(int kitNumber) => kitRoots[kitNumber - 1];

        public TreeNode GetTriggerRoot(int kitNumber, int trigger)
        {
            var kitRoot = GetKitRoot(kitNumber);
            return kitRoot.ResolveNode(string.Format(triggerPathFormat, trigger));
        }

        internal (FieldContainer, InstrumentField) GetMainInstrumentField(int kitNumber, int trigger)
        {
            var kitRoot = GetKitRoot(kitNumber);
            var (container, field) = kitRoot.Container.ResolveField(string.Format(mainInstrumentPathFormat, trigger));
            return (container, (InstrumentField) field);
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