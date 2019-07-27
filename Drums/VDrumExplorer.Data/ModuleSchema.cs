using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Json;

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
        public IReadOnlyDictionary<ModuleAddress, IPrimitiveField> PrimitiveFieldsByAddress { get; }
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
            PrimitiveFieldsByAddress = Root.DescendantsAndSelf().OfType<IPrimitiveField>().ToDictionary(f => f.Address).AsReadOnly();
            VisualRoot = visualRoot;
            //FieldsByPath = Root.DescendantsAndSelf().ToDictionary(f => f.Path).AsReadOnly();
        }

        public static ModuleSchema FromAssemblyResources(Assembly assembly, string resourceBase, string resourceName) =>
            FromJson(JsonLoader.FromAssemblyResources(assembly, resourceBase).LoadResource(resourceName));

        public static ModuleSchema FromDirectory(string path, string resourceName) =>
            FromJson(JsonLoader.FromDirectory(path).LoadResource(resourceName));

        private static ModuleSchema FromJson(JObject json) =>
            ModuleJson.FromJson(json).ToModuleSchema();
    }
}
