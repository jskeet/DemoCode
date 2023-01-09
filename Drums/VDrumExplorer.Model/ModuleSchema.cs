// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDrumExplorer.Model.Midi;
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

        static ModuleSchema()
        {
            var builder = new Dictionary<ModuleIdentifier, Lazy<ModuleSchema>>();
            AddSchema(ModuleIdentifier.AE01);
            AddSchema(ModuleIdentifier.AE10);
            AddSchema(ModuleIdentifier.TD07);
            AddSchema(ModuleIdentifier.TD17, 0x01);
            AddSchema(ModuleIdentifier.TD27, 0x02);
            AddSchema(ModuleIdentifier.TD50);
            AddSchema(ModuleIdentifier.TD50X);

            void AddSchema(ModuleIdentifier identifier, params int[] additionalSoftwareRevisions)
            {
                string name = identifier.Name.Replace("-", "");
                string resourceBase = $"SchemaResources.{name}";
                string resourceName = $"{name}.json";
                builder.Add(identifier, LazyFromAssemblyResources(resourceBase, resourceName, identifier.SoftwareRevision));

                foreach (var softwareRevision in additionalSoftwareRevisions)
                {
                    var newIdentifier = identifier.WithSoftwareRevision(softwareRevision);
                    builder.Add(newIdentifier, LazyFromAssemblyResources(resourceBase, resourceName, softwareRevision));
                }
            }
            KnownSchemas = builder.AsReadOnly();
        }

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
        internal string? KitSubNamePath { get; }

        private ModuleSchema(ModuleJson json)
        {
            // Note: we populate everything other than the fields first, so that field
            // construction can rely on it.
            json.Validate();
            Identifier = json.Identifier!.ToModuleIdentifier();
            Kits = json.Counts!["kits"];
            UserSamples = json.Counts!["userSamples"];
            var instrumentGroups = json.InstrumentGroups.Select((igj, index) => igj.ToInstrumentGroup(index));
            if (UserSamples != 0)
            {
                instrumentGroups = instrumentGroups.Concat(new[] { InstrumentGroup.ForUserSamples(json.InstrumentGroups!.Count, UserSamples) });
            }
            InstrumentGroups = instrumentGroups.ToReadOnlyList();

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

            UserSampleInstruments = UserSamples == 0 ? new List<Instrument>().AsReadOnly() : InstrumentGroups.Last().Instruments;

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
            KitSubNamePath = json.KitSubNamePath;
            triggerPathFormat = json.TriggerPathFormat!;
        }

        public TreeNode GetKitRoot(int kitNumber) => kitRoots[kitNumber - 1];

        public TreeNode GetTriggerRoot(int kitNumber, int trigger)
        {
            var kitRoot = GetKitRoot(kitNumber);
            return kitRoot.ResolveNode(string.Format(triggerPathFormat, trigger));
        }

        internal InstrumentField GetMainInstrumentField(int kitNumber, int trigger)
        {
            var kitRoot = GetKitRoot(kitNumber);
            var field = kitRoot.Container.ResolveField(string.Format(mainInstrumentPathFormat, trigger));
            return (InstrumentField) field;
        }

        private static Lazy<ModuleSchema> LazyFromAssemblyResources(string resourceBase, string resourceName, int softwareRevision) =>
            new Lazy<ModuleSchema>(() => FromAssemblyResources(typeof(ModuleSchema).Assembly, resourceBase, resourceName, softwareRevision));

        public static ModuleSchema FromAssemblyResources(Assembly assembly, string resourceBase, string resourceName, int softwareRevision) =>
            FromJson(JsonLoader.FromAssemblyResources(assembly, resourceBase).LoadResource(resourceName, softwareRevision));

        public static ModuleSchema FromDirectory(string path, string resourceName, int softwareRevision) =>
            FromJson(JsonLoader.FromDirectory(path).LoadResource(resourceName, softwareRevision));

        private static ModuleSchema FromJson(JObject json) =>
            new ModuleSchema(ModuleJson.FromJson(json));

    }
}