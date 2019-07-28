using Newtonsoft.Json.Linq;
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
        public int MidiId { get; }

        public Container Root { get; set; }

        public IReadOnlyDictionary<int, Instrument> InstrumentsById { get; }
        public IReadOnlyList<Instrument> Instruments { get; }
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
        /// The root of the visual layout for the module.
        /// </summary>
        public VisualTreeNode VisualRoot { get; }

        internal ModuleSchema(string name, int midiId, Container root, IReadOnlyList<InstrumentGroup> instrumentGroups, VisualTreeNode visualRoot)
        {
            Name = name;
            MidiId = midiId;
            Root = root;
            InstrumentGroups = instrumentGroups;
            Instruments = InstrumentGroups.SelectMany(ig => ig.Instruments)
                .OrderBy(i => i.Id)
                .ToList()
                .AsReadOnly();
            InstrumentsById = Instruments.ToDictionary(i => i.Id).AsReadOnly();
            PrimitiveFieldsByAddress = Root.DescendantsAndSelf().OfType<IPrimitiveField>()
                .ToDictionary(f => f.Address)
                .AsReadOnly();
            VisualRoot = visualRoot;
            ParentsByField = BuildParentsByField(Root);
        }

        public static ModuleSchema FromAssemblyResources(Assembly assembly, string resourceBase, string resourceName) =>
            FromJson(JsonLoader.FromAssemblyResources(assembly, resourceBase).LoadResource(resourceName));

        public static ModuleSchema FromDirectory(string path, string resourceName) =>
            FromJson(JsonLoader.FromDirectory(path).LoadResource(resourceName));

        private static ModuleSchema FromJson(JObject json) =>
            ModuleJson.FromJson(json).ToModuleSchema();

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
