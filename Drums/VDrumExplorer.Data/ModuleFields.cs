using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using VDrumExplorer.Models.Fields;
using VDrumExplorer.Models.Json;
using static System.FormattableString;

namespace VDrumExplorer.Models
{
    // TODO: Rename this.

    /// <summary>
    /// Top-level information about a module, breaking down into kits.
    /// </summary>
    public sealed class ModuleFields
    {
        private const string ContainerPrefix = "container:";
        
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

        private ModuleFields(ModuleJson moduleJson)
        {
            var converter = new ModuleConverter(moduleJson);
            Name = moduleJson.Name;
            MidiId = moduleJson.MidiId.Value;

            if (!converter.ContainersByName.TryGetValue("Root", out var rootJson))
            {
                throw new ArgumentException($"No Root container defined");
            }
            Root = converter.ToContainer(rootJson, "Root", "", new ModuleAddress(0));
            InstrumentGroups = moduleJson.InstrumentGroups
                .Select((igj, index) => new InstrumentGroup(igj.Description, index, igj.Instruments))
                .ToList()
                .AsReadOnly();
            Instruments = InstrumentGroups.SelectMany(ig => ig.Instruments)
                .OrderBy(i => i.Id)
                .ToList()
                .AsReadOnly();
            InstrumentsById = Instruments.ToDictionary(i => i.Id).AsReadOnly();
            PrimitiveFieldsByAddress = Root.DescendantsAndSelf().OfType<IPrimitiveField>().ToDictionary(f => f.Address).AsReadOnly();
        }

        public static ModuleFields FromAssemblyResources(Assembly assembly, string resourceBase, string resourceName) =>
            FromJson(JsonLoader.FromAssemblyResources(assembly, resourceBase).LoadResource(resourceName));

        public static ModuleFields FromDirectory(string path, string resourceName) =>
            FromJson(JsonLoader.FromDirectory(path).LoadResource(resourceName));

        private static ModuleFields FromJson(JObject json) =>
            new ModuleFields(ModuleJson.FromJson(json));

        private sealed class ModuleConverter
        {
            internal ModuleJson ModuleJson { get; }
            internal Dictionary<string, ContainerJson> ContainersByName { get; }

            internal ModuleConverter(ModuleJson moduleJson)
            {
                ModuleJson = moduleJson;               
                ContainersByName = moduleJson.Containers.ToDictionary(c => c.Name);
            }

            internal Container ToContainer(ContainerJson containerJson, string description, string path, ModuleAddress address)
            {
                List<Fields.IField> fields = containerJson.Fields
                    .SelectMany(fieldJson => ToFields(fieldJson, path, address))
                    .ToList();
                int size = containerJson.Size?.Value ?? ((fields.Last().Address + fields.Last().Size) - address);

                return new Container(containerJson.Name, description, path, address, size, fields.AsReadOnly());
            }

            internal IEnumerable<IField> ToFields(FieldJson fieldJson, string parentPath, ModuleAddress parentAddress)
            {
                int? repeat = fieldJson.GetRepeat(ModuleJson);
                ModuleAddress address = parentAddress + fieldJson.Offset.Value;
                if (repeat == null)
                {
                    string path = $"{parentPath}/{fieldJson.Description}";
                    yield return ToField(fieldJson, path, fieldJson.Description, address);
                }
                else
                {
                    for (int i = 0; i < repeat; i++)
                    {
                        string description = Invariant($"{fieldJson.Description} ({i + 1})");
                        string path = Invariant($"{parentPath}/{description}");
                        yield return ToField(fieldJson, path, description, address);
                        address += fieldJson.Gap.Value;
                    }
                }
            }

            private IField ToField(FieldJson fieldJson, string path, string description, ModuleAddress address)
            {
                return fieldJson.Type switch
                {
                    "boolean" => (Fields.IField) new BooleanField(description, path, address, 1),
                    "boolean32" => (Fields.IField) new BooleanField(description, path, address, 4),
                    "range8" => BuildRangeField(1),
                    "range16" => BuildRangeField(2),
                    "range32" => BuildRangeField(4),
                    "enum" => new EnumField(description, path, address, 1, fieldJson.Values.AsReadOnly()),
                    "enum32" => new EnumField(description, path, address, 4, fieldJson.Values.AsReadOnly()),
                    "dynamicOverlay" => BuildDynamicOverlay(),
                    "instrument" => new InstrumentField(description, path, address, 4),
                    "musicalNote" => new MusicalNoteField(description, path, address, 4),
                    "volume32" => new Volume32Field(description, path, address),
                    "string" => new StringField(description, path, address, fieldJson.Length.Value),
                    string text when text.StartsWith(ContainerPrefix) => BuildContainer(),
                    _ => throw new InvalidOperationException($"Unknown field type: {fieldJson.Type}")
                };

                DynamicOverlay BuildDynamicOverlay()
                {
                    // Offsets within each container are relative to the parent container of this field,
                    // not relative to this field itself.
                    ModuleAddress parentAddress = address - fieldJson.Offset.Value;
                    var overlayJson = fieldJson.DynamicOverlay;
                    ModuleAddress switchAddress = parentAddress + overlayJson.SwitchOffset.Value;
                    var containers = overlayJson.Containers
                        .Select(json => ToContainer(json, description, path, parentAddress))
                        .ToList()
                        .AsReadOnly();
                    return new DynamicOverlay(description, path, address, overlayJson.Size.Value, switchAddress, overlayJson.SwitchTransform, containers);
                }

                Container BuildContainer()
                {
                    string containerName = fieldJson.Type.Substring(ContainerPrefix.Length);
                    var containerJson = ContainersByName[containerName];
                    return ToContainer(containerJson, description, path, address);
                }

                RangeField BuildRangeField(int size) =>
                    new RangeField(
                        description, path, address, size,
                        fieldJson.Min, fieldJson.Max, fieldJson.Off, fieldJson.Divisor, fieldJson.Multiplier, fieldJson.ValueOffset,
                        fieldJson.Suffix);
            }
        }
    }
}
