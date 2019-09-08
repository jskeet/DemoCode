// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// Description to display. For repeated fields, this is a format string,
        /// where {0} will be replaced with the element index, and {1} will be replaced
        /// with the value from <see cref="DescriptionLookup"/>, if any.
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
        /// The label for the <see cref="Off"/>; defaults to "off".
        /// </summary>
        public string OffLabel { get; set; } = "Off";

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
        /// For repeated fields only, a lookup to provide format values.
        /// </summary>
        public string? DescriptionLookup { get; set; }

        /// <summary>
        /// For instrument fields only, the offset of the bank switch field from the
        /// instrument field.
        /// </summary>
        public HexInt32? BankOffset { get; set; }

        /// <summary>
        /// The details for a DynamicOverlay field.
        /// </summary>
        public DynamicOverlayJson? DynamicOverlay { get; set; }

        /// <summary>
        /// If set, the condition for the field to be enabled.
        /// </summary>
        public FieldConditionJson? Condition { get; set; }
        
        public override string ToString() => Description ?? "(No description)";

        internal IEnumerable<IField> ToFields(ModuleSchema schema, ModuleJson module)
        {
            string description = ValidateNotNull(Description, nameof(Description));
            string name = Name ?? description;
            int? repeat = module.GetCount(Repeat);
            var offset = ValidateNotNull(Offset, nameof(Offset)).Value;
            FieldCondition? condition = GetCondition();
            if (repeat == null)
            {
                yield return ToField(schema, module, name, offset, description, condition);
            }
            else
            {
                var gap = ValidateNotNull(Gap, nameof(Gap)).Value;
                List<string>? lookup = null;
                if (DescriptionLookup != null)
                {
                    ValidateNotNull(module.Lookups, nameof(module.Lookups));
                    lookup = module.Lookups.FirstOrDefault(candidate => candidate.Name == DescriptionLookup)?.Values;
                    Validate(lookup != null, "Lookup ${DescriptionLookup} not found for descriptions.");
                    Validate(lookup!.Count == repeat,
                        $"Lookup ${DescriptionLookup} has {lookup.Count} elements; field has repeats {repeat} times.");
                }
                for (int i = 1; i <= repeat; i++)
                {
                    string indexedName = Invariant($"{name}[{i}]");
                    string indexValue = i.ToString(CultureInfo.InvariantCulture);
                    string? lookupValue = lookup?[i - 1];
                    string fullDescription = string.Format(Description, indexValue, lookupValue);
                    yield return ToField(schema, module, indexedName, offset, fullDescription, condition);
                    offset += gap;
                }
            }

            FieldCondition? GetCondition()
            {
                if (Condition is null)
                {
                    return null;
                }
                int conditionOffset = ValidateNotNull(Condition.Offset, nameof(Condition.Offset)).Value;
                int requiredValue = ValidateNotNull(Condition.RequiredValue, nameof(Condition.RequiredValue));
                return new FieldCondition(conditionOffset, requiredValue);
            }
        }

        private IField ToField(ModuleSchema schema, ModuleJson module, string name, int offset, string description, FieldCondition? condition)
        {
            // TODO: Validate that we don't have "extra" parameters?
            return Type switch
            {
                "boolean" => (IField) new BooleanField(BuildCommon(1)),
                "boolean32" => new BooleanField(BuildCommon(4)),
                "range8" => BuildNumericField(1),
                "range16" => BuildNumericField(2),
                "range32" => BuildNumericField(4),
                "enum" => BuildEnumField(1),
                "enum16" => BuildEnumField(2),
                "enum32" => BuildEnumField(4),
                "dynamicOverlay" => BuildDynamicOverlay(),
                "instrument" => new InstrumentField(BuildCommon(4), ValidateNotNull(BankOffset, nameof(BankOffset)).Value),
                "musicalNote" => new EnumField(BuildCommon(4), MusicalNoteValues, 0),
                "volume32" => new NumericField(BuildCommon(4), -601, 60, 10, null, 0, "dB", (-601, "INF")),
                "string" => BuildStringField(1),
                "string16" => BuildStringField(2),
                "midi32" => new MidiNoteField(BuildCommon(4)),
                string text when text.StartsWith(ContainerPrefix) => BuildContainer(),
                _ => throw new InvalidOperationException($"Unknown field type: {Type}")
            };

            EnumField BuildEnumField(int size) =>
                new EnumField(BuildCommon(size), ValidateNotNull(Values, nameof(Values)).AsReadOnly(), Min ?? 0);

            StringField BuildStringField(int bytesPerChar)
            {
                var length = ValidateNotNull(Length, nameof(Length));
                var common = BuildCommon(length * bytesPerChar);
                return new StringField(common, length);
            }
            
            DynamicOverlay BuildDynamicOverlay()
            {
                // Offsets within each container are relative to the parent container of this field,
                // not relative to this field itself.
                var overlay = ValidateNotNull(DynamicOverlay, nameof(DynamicOverlay));
                var switchOffset = ValidateNotNull(overlay.SwitchOffset, nameof(overlay.SwitchOffset)).Value;
                var containers = ValidateNotNull(overlay.Containers, nameof(overlay.Containers))
                    .Select((json, index) => json.ToContainer(schema, module, Invariant($"Overlay[{index}]"), 0, description, condition: null))
                    .ToList()
                    .AsReadOnly();
                var size = ValidateNotNull(overlay.Size, nameof(overlay.Size));
                var common = new FieldBase.Parameters(schema, name, offset, size.Value, description, condition: null);
                return new DynamicOverlay(common, switchOffset, overlay.SwitchTransform, containers);
            }

            Container BuildContainer()
            {
                string containerName = Type!.Substring(ContainerPrefix.Length);
                var containerJson = module.Containers![containerName];
                return containerJson.ToContainer(schema, module, name, offset, description, condition);
            }

            NumericField BuildNumericField(int size)
            {
                var min = ValidateNotNull(Min, nameof(Min));
                var max = ValidateNotNull(Max, nameof(Max));
                return new NumericField(BuildCommon(size),
                    min, max,
                    Divisor, Multiplier, ValueOffset, Suffix,
                    Off == null ? default((int, string)?) : (Off.Value, OffLabel));
            }

            FieldBase.Parameters BuildCommon(int size) => new FieldBase.Parameters(schema, name, offset, size, description, condition);
        }
    }
}
