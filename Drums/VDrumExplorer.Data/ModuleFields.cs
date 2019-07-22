using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Json;
using static System.FormattableString;

namespace VDrumExplorer.Data
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
        public IReadOnlyDictionary<string, IField> FieldsByPath { get; }
        public VisualTreeNode VisualRoot { get; }

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
            FieldsByPath = Root.DescendantsAndSelf().ToDictionary(f => f.Path);
            var visualTreeConverter = new VisualTreeConverter(moduleJson, FieldsByPath);
            VisualRoot = visualTreeConverter.ConvertVisualNodes(moduleJson.VisualTree, "", new Dictionary<string, string>()).Single();
        }

        public static ModuleFields FromAssemblyResources(Assembly assembly, string resourceBase, string resourceName) =>
            FromJson(JsonLoader.FromAssemblyResources(assembly, resourceBase).LoadResource(resourceName));

        public static ModuleFields FromDirectory(string path, string resourceName) =>
            FromJson(JsonLoader.FromDirectory(path).LoadResource(resourceName));

        private static ModuleFields FromJson(JObject json) =>
            new ModuleFields(ModuleJson.FromJson(json));

        private static string CombinePaths(string currentPath, string suffix)
        {
            if (suffix == ".")
            {
                return currentPath;
            }
            if (currentPath == "")
            {
                return suffix;
            }
            return $"{currentPath}/{suffix}";
        }
        
        private static int? GetRepeat(ModuleJson moduleJson, string repeat)
        {
            switch (repeat)
            {
                case null: return null;
                case "$kits": return moduleJson.Kits;
                case "$instruments": return moduleJson.InstrumentsPerKit;
                case "$triggers": return moduleJson.Triggers;
                default:
                    if (!int.TryParse(repeat, NumberStyles.None, CultureInfo.InvariantCulture, out var result))
                    {
                        throw new InvalidOperationException($"Invalid repeat value: '{repeat}'");
                    }
                    return result;
            }
        }

        private sealed class VisualTreeConverter
        {
            private readonly ModuleJson moduleJson;
            private readonly IReadOnlyDictionary<string, IField> fieldsByPath;

            internal VisualTreeConverter(ModuleJson moduleJson, IReadOnlyDictionary<string, IField> fieldsByPath) =>
                (this.moduleJson, this.fieldsByPath) = (moduleJson, fieldsByPath);

            internal IEnumerable<VisualTreeNode> ConvertVisualNodes(VisualTreeNodeJson treeJson, string parentPath, IDictionary<string, string> indexes)
            {
                int? repeat = GetRepeat(moduleJson, treeJson.Repeat);
                if (repeat == null)
                {
                    string nodePath = CombinePaths(parentPath, ReplaceIndexes(treeJson.Path, indexes));
                    yield return ToVisualTreeNode(treeJson, nodePath, indexes, treeJson.Description, null);
                }
                else
                {
                    for (int i = 1; i <= repeat; i++)
                    {
                        Dictionary<string, string> newIndexes = new Dictionary<string, string>(indexes) { { treeJson.Index, i.ToString(CultureInfo.InvariantCulture) } };
                        string nodePath = CombinePaths(parentPath, ReplaceIndexes(treeJson.Path, newIndexes));

                        yield return ToVisualTreeNode(treeJson, nodePath, newIndexes, null, BuildFormatElement(treeJson.Format, parentPath, treeJson.FormatPaths, newIndexes));
                    }
                }
            }

            private VisualTreeDetail.FormatElement BuildFormatElement(string formatString, string parentPath, IEnumerable<string> formatPaths, IDictionary<string, string> indexes)
            {
                formatString = ReplaceIndexes(formatString, indexes);
                var formatFields = formatPaths
                    .Select(p => CombinePaths(parentPath, ReplaceIndexes(p, indexes)))
                    .Select(p => fieldsByPath[p])
                    .ToList()
                    .AsReadOnly();
                return new VisualTreeDetail.FormatElement(formatString, formatFields);
            }

            private VisualTreeNode ToVisualTreeNode(VisualTreeNodeJson treeJson, string nodePath, IDictionary<string, string> indexes, string description, VisualTreeDetail.FormatElement formatElement)
            {
                var children = treeJson.Children.SelectMany(child => ConvertVisualNodes(child, nodePath, indexes)).ToList().AsReadOnly();
                var details = treeJson.Details.Select(detail => ToVisualTreeDetail(detail, nodePath, indexes)).ToList().AsReadOnly();
                return new VisualTreeNode(children, details, description, formatElement);
            }

            private VisualTreeDetail ToVisualTreeDetail(VisualTreeDetailJson detailJson, string parentPath, IDictionary<string, string> indexes)
            {
                int? repeat = GetRepeat(moduleJson, detailJson.Repeat);
                if (repeat == null)
                {
                    string containerPath = CombinePaths(parentPath, ReplaceIndexes(detailJson.Path, indexes));
                    var container = (Container) fieldsByPath[containerPath];
                    return new VisualTreeDetail(detailJson.Description, container, null);
                }
                else
                {
                    List<VisualTreeDetail.FormatElement> formatElements = new List<VisualTreeDetail.FormatElement>();
                    for (int i = 1; i <= repeat; i++)
                    {
                        Dictionary<string, string> newIndexes = new Dictionary<string, string>(indexes) { { detailJson.Index, i.ToString(CultureInfo.InvariantCulture) } };
                        var formatElement = BuildFormatElement(detailJson.Format, parentPath, detailJson.FormatPaths, newIndexes);
                        formatElements.Add(formatElement);
                    }
                    return new VisualTreeDetail(detailJson.Description, null, formatElements);
                }
            }

            private static string ReplaceIndexes(string text, IDictionary<string, string> indexes)
            {
                foreach (var pair in indexes)
                {
                    text = text.Replace("$" + pair.Key, pair.Value);
                }
                return text;
            }
        }

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
                string name = fieldJson.Name ?? fieldJson.Description;
                int? repeat = GetRepeat(ModuleJson, fieldJson.Repeat);
                ModuleAddress address = parentAddress + fieldJson.Offset.Value;
                if (repeat == null)
                {
                    string path = CombinePaths(parentPath, name);
                    yield return ToField(fieldJson, path, fieldJson.Description, address);
                }
                else
                {
                    for (int i = 1; i <= repeat; i++)
                    {
                        string description = Invariant($"{fieldJson.Description} ({i})");
                        string path = CombinePaths(parentPath,  Invariant($"{name}[{i}]"));
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
