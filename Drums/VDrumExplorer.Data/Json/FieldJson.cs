using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Data.Fields;
using static System.FormattableString;

namespace VDrumExplorer.Data.Json
{
    using static Validation;

    internal class FieldJson
    {
        private const string ContainerPrefix = "container:";
        private static readonly IReadOnlyList<string> MusicalNoteValues = new List<string>
        {
            "Hemidemisemiquaver triplet",
            "Hemidemisemiquaver",
            "Demisemiquaver triplet",
            "Demisemiquaver",
            "Semiquaver triplet",
            "Dotted demisemiquaver",
            "Semiquaver",
            "Quaver triplet",
            "Dotted semiquaver",
            "Quaver",
            "Crotchet triplet",
            "Dotted quaver",
            "Crotchet",
            "Minim triplet",
            "Dotted crotchet",
            "Minim",
            "Semibreve triplet",
            "Dotted minim",
            "Semibreve",
            "Breve triplet",
            "Dotted semibreve",
            "Breve",
        }.AsReadOnly();

        /// <summary>
        /// Developer-oriented comment. Has no effect.
        /// </summary>
        public string? Comment { get; set; }
        
        /// <summary>
        /// Description to display.
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Name of the field in the path; defaults to <see cref="Description"/>.
        /// </summary>
        public string? Name { get; set; }

        public HexInt32? Offset { get; set; }

        /// <summary>
        /// The type of the field. If this begins with "container:" then the
        /// text following the prefix is the container name.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// The length of the field, for strings.
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// The maximum valid value (in raw form).
        /// </summary>
        public int? Max { get; set; }

        /// <summary>
        /// The minimum valid value (in raw form).
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        /// The numeric value representing "off".
        /// </summary>
        public int? Off { get; set; }

        /// <summary>
        /// Amount to divide the value by, for ranges (e.g. 10 for a 0, 0.1, 0.2 etc value).
        /// </summary>
        public int? Divisor { get; set; }

        /// <summary>
        /// Amount to multiply the value by, for ranges (e.g. 2 for a 0, 2, 4 etc value).
        /// </summary>
        public int? Multiplier { get; set; }

        /// <summary>
        /// The suffix to apply, usually a unit e.g. "dB".
        /// </summary>
        public string? Suffix { get; set; }

        /// <summary>
        /// The amount to add to the stored value to get the displayed value. This is applied
        /// before multiplication or division.
        /// </summary>
        public int? ValueOffset { get; set; }

        /// <summary>
        /// The values (from 0 upwards) for enum fields.
        /// </summary>
        public List<string>? Values { get; set; }

        /// <summary>
        /// The number of times this field is repeated. May be numeric, or
        /// a variable value such as "$kits" or "$instruments".
        /// </summary>
        public string? Repeat { get; set; }

        /// <summary>
        /// The gap between repeated fields (from start to start).
        /// </summary>
        public HexInt32? Gap { get; set; }

        /// <summary>
        /// The details for a DynamicOverlay field.
        /// </summary>
        public DynamicOverlayJson? DynamicOverlay { get; set; }

        public override string ToString() => Description ?? "(No description)";

        internal IEnumerable<IField> ToFields(ModuleJson module, FieldPath parentPath, ModuleAddress parentAddress)
        {
            string description = ValidateNotNull(parentPath + "???", Description, nameof(Description));
            string name = Name ?? description;
            FieldPath path = parentPath + name;
            int? repeat = module.GetRepeat(Repeat);
            var offset = ValidateNotNull(path, Offset, nameof(Offset));
            ModuleAddress address = parentAddress + offset.Value;
            if (repeat == null)
            {
                yield return ToField(module, path, description, address);
            }
            else
            {
                var gap = ValidateNotNull(parentPath, Gap, nameof(Gap)).Value;
                for (int i = 1; i <= repeat; i++)
                {
                    string fullDescription = Invariant($"{description} ({i})");
                    yield return ToField(module, path.WithIndex(i), fullDescription, address);
                    address += gap;
                }
            }
        }

        private IField ToField(ModuleJson module, FieldPath path, string description, ModuleAddress address)
        {
            // TODO: Validate that we don't have "extra" parameters?
            return Type switch
            {
                "boolean" => (IField) new BooleanField(path, address, 1, description),
                "boolean32" => new BooleanField(path, address, 4, description),
                "range8" => BuildNumericField(1),
                "range16" => BuildNumericField(2),
                "range32" => BuildNumericField(4),
                "enum" => new EnumField(path, address, 1, description, ValidateNotNull(path, Values, nameof(Values)).AsReadOnly()),
                "enum32" => new EnumField(path, address, 4, description, ValidateNotNull(path, Values, nameof(Values)).AsReadOnly()),
                "dynamicOverlay" => BuildDynamicOverlay(),
                "instrument" => new InstrumentField(path, address, 4, description),
                "musicalNote" => new EnumField(path, address, 4, description, MusicalNoteValues),
                "volume32" => new NumericField(path, address, 4, description, -601, 60, 10, null, 0, "dB", (-601, "INF")),
                "string" => new StringField(path, address, ValidateNotNull(path, Length, nameof(Length)), description),
                string text when text.StartsWith(ContainerPrefix) => BuildContainer(),
                _ => throw new InvalidOperationException($"Unknown field type: {Type}")
            };

            DynamicOverlay BuildDynamicOverlay()
            {
                // Offsets within each container are relative to the parent container of this field,
                // not relative to this field itself.
                ModuleAddress parentAddress = address - Offset!.Value;
                var overlay = ValidateNotNull(path, DynamicOverlay, nameof(DynamicOverlay));
                var switchOffset = ValidateNotNull(path, overlay.SwitchOffset, nameof(overlay.SwitchOffset));
                ModuleAddress switchAddress = parentAddress + switchOffset.Value;
                var containers = ValidateNotNull(path, overlay.Containers, nameof(overlay.Containers))
                    .Select(json => json.ToContainer(module, path, parentAddress, description))
                    .ToList()
                    .AsReadOnly();
                return new DynamicOverlay(
                    path, address, ValidateNotNull(path, overlay.Size, nameof(overlay.Size)).Value,
                    description, switchAddress, overlay.SwitchTransform, containers);
            }

            Container BuildContainer()
            {
                string containerName = Type!.Substring(ContainerPrefix.Length);
                var containerJson = module.FindContainer(path, containerName);
                return containerJson.ToContainer(module, path, address, description);
            }

            NumericField BuildNumericField(int size)
            {
                var min = ValidateNotNull(path, Min, nameof(Min));
                var max = ValidateNotNull(path, Max, nameof(Max));
                return new NumericField(
                    path, address, size, description, min, max,
                    Divisor, Multiplier, ValueOffset, Suffix,
                    Off == null ? default((int, string)?) : (Off.Value, "Off"));
            }
        }
    }
}
