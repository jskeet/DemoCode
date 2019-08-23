﻿// Copyright 2019 Jon Skeet. All rights reserved.
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
        /// The name of the module, e.g. "TD-17".
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The ID of the module.
        /// </summary>
        public int ModelId { get; }
        
        /// <summary>
        /// The family code as reported by a Midi identity response.
        /// </summary>
        public int FamilyCode { get; }

        /// <summary>
        /// The family number code as reported by a Midi identity response.
        /// </summary>
        public int FamilyNumberCode { get; }

        public Container Root { get; set; }

        public IReadOnlyList<Instrument> PresetInstruments { get; }
        public IReadOnlyList<Instrument> UserSampleInstruments { get; }
        public IReadOnlyList<InstrumentGroup> InstrumentGroups { get; }
        
        /// <summary>
        /// Mapping from ModuleAddress to primitive fields. Only non-overlaid fields are included.
        /// </summary>
        public IReadOnlyDictionary<ModuleAddress, IPrimitiveField> PrimitiveFieldsByAddress { get; }
        
        /// <summary>
        /// Backlinks from each field to its parent container, including the fields within overlaid containers.
        /// The overlaid container themselves do not (currently) link back to the DynamicOverlay they're part of.
        /// </summary>
        public IReadOnlyDictionary<IField, Container> ParentsByField { get; }
        
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
            Name = json.Name!;
            ModelId = json.ModelId!.Value;
            FamilyCode = json.FamilyCode!.Value;
            FamilyNumberCode = json.FamilyNumberCode!.Value;
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
            Root = json.BuildRootContainer(this);
            LogicalRoot = json.BuildLogicalRoot(Root);
            PrimitiveFieldsByAddress = Root.DescendantsAndSelf().OfType<IPrimitiveField>()
                .ToDictionary(f => f.Address)
                .AsReadOnly();
            ParentsByField = BuildParentsByField(Root);
            PhysicalRoot = VisualTreeNode.FromContainer(Root);
        }

        public static ModuleSchema FromAssemblyResources(Assembly assembly, string resourceBase, string resourceName) =>
            FromJson(JsonLoader.FromAssemblyResources(assembly, resourceBase).LoadResource(resourceName));

        public static ModuleSchema FromDirectory(string path, string resourceName) =>
            FromJson(JsonLoader.FromDirectory(path).LoadResource(resourceName));

        private static ModuleSchema FromJson(JObject json) =>
            new ModuleSchema(ModuleJson.FromJson(json));

        private static IReadOnlyDictionary<IField, Container> BuildParentsByField(Container root)
        {
            var dictionary = new Dictionary<IField, Container>();
            new ParentBuildingVisitor(dictionary).Visit(root);
            return dictionary.AsReadOnly();
        }

        private class ParentBuildingVisitor : FieldVisitor
        {
            private readonly IDictionary<IField, Container> dictionary;

            internal ParentBuildingVisitor(IDictionary<IField, Container> dictionary) =>
                this.dictionary = dictionary;

            public override void VisitContainer(Container container)
            {
                base.VisitContainer(container);
                foreach (var field in container.Fields)
                {
                    dictionary[field] = container;
                }
            }

            public override void VisitDynamicOverlay(DynamicOverlay overlay)
            {
                base.VisitDynamicOverlay(overlay);
                foreach (var container in overlay.OverlaidContainers)
                {
                    Visit(container);
                }
            }
        }
    }
}
