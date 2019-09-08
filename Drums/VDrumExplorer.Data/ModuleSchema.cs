// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Json;
using VDrumExplorer.Data.Layout;

namespace VDrumExplorer.Data
{
    /// <summary>
    /// Schema for a module, containing the layout of the fields that
    /// can be populated by a <see cref="ModuleData"/>.
    /// </summary>
    public sealed class ModuleSchema
    {
        /// <summary>
        /// The identifier for the module.
        /// </summary>
        public ModuleIdentifier Identifier { get; }

        public FixedContainer Root { get; set; }

        public IReadOnlyList<Instrument> PresetInstruments { get; }
        public IReadOnlyList<Instrument> UserSampleInstruments { get; }
        public IReadOnlyList<InstrumentGroup> InstrumentGroups { get; }
        
        /// <summary>
        /// Root of the visual tree for the module using the logical layout.
        /// </summary>
        public VisualTreeNode LogicalRoot { get; }
        
        /// <summary>
        /// Root of the visual tree for the module using the physical layout.
        /// </summary>
        public VisualTreeNode PhysicalRoot { get; }

        // Note: this used to be in ModuleJson.ToModuleSchema(), but it turns out it's really useful for
        // a field to have access to the schema it's part of... which is tricky when everything is immutable
        // and the schema also has to have references to the fields. So this code is ugly - and makes field testing
        // trickier - but at least it's pleasant to use elsewhere.
        internal ModuleSchema(ModuleJson json)
        {
            // Note: we populate everything other than the fields first, so that field
            // construction can rely on it.
            json.Validate();
            Identifier = new ModuleIdentifier(json.Name!, json.ModelId!.Value, json.FamilyCode!.Value, json.FamilyNumberCode!.Value);
            InstrumentGroups = json.BuildInstrumentGroups();
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

            UserSampleInstruments = Enumerable.Range(0, json.UserSamples!.Value)
                .Select(id => Instrument.FromUserSample(id))
                .ToList()
                .AsReadOnly();
            
            // Now do everything with the fields.
            Root = new FixedContainer(json.BuildRootContainer(this), new ModuleAddress(0));
            LogicalRoot = json.BuildLogicalRoot(Root);
            PhysicalRoot = VisualTreeNode.FromFixedContainer(Root);
        }

        public static ModuleSchema FromAssemblyResources(Assembly assembly, string resourceBase, string resourceName) =>
            FromJson(JsonLoader.FromAssemblyResources(assembly, resourceBase).LoadResource(resourceName));

        public static ModuleSchema FromDirectory(string path, string resourceName) =>
            FromJson(JsonLoader.FromDirectory(path).LoadResource(resourceName));

        private static ModuleSchema FromJson(JObject json) =>
            new ModuleSchema(ModuleJson.FromJson(json));

        public IEnumerable<AnnotatedContainer> GetContainers()
        {
            var queue = new Queue<AnnotatedContainer>();
            queue.Enqueue(new AnnotatedContainer("", Root));
            while (queue.Count != 0)
            {
                var current = queue.Dequeue();
                yield return current;
                foreach (var container in current.Container.Fields.OfType<Container>())
                {
                    queue.Enqueue(current.AnnotateChildContainer(container));
                }
            }
        }
    }
}
