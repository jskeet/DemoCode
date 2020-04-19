// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

#nullable disable warnings

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Utility;
using static System.FormattableString;

namespace VDrumExplorer.Model.Schema.Json
{
    using static Preconditions;
    using static Validation;

    internal class FieldJson : ContainerItemBase
    {
        // FIXME: Work out where to put this.
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
        /// The type of the field.
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
        /// The default value, if not 0.
        /// </summary>
        public int? Default { get; set; }

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
        /// For instrument fields only, the offset of the bank switch field, in the same container.
        /// </summary>
        public HexInt32? BankOffset { get; set; }

        /// <summary>
        /// If set, the condition for the field to be enabled.
        /// </summary>
        public FieldConditionJson? Condition { get; set; }

        /// <summary>
        /// For "overlay" fields only, the details of the dynamic overlay.
        /// </summary>
        public DynamicOverlayJson? Overlay { get; set; }

        public override string ToString() => Description ?? Name ?? "(No description)";

        public IEnumerable<IField> ToFields(ModuleJson module, ModuleOffset offset)
        {
            AssertNotNull(ResolvedName);
            AssertNotNull(ResolvedDescription);

            if (Repeat is null)
            {
                yield return ToField(module, ResolvedName, ResolvedDescription, offset);
            }
            else
            {
                Preconditions.AssertNotNull(Repeat.Items);
                var variables = SchemaVariables.Empty;
                int? gap = Repeat.Gap?.Value;
                foreach (var tuple in module.GetRepeatSequence(Repeat.Items, variables))
                {
                    var itemVariables = variables.WithVariable(Repeat.IndexVariable, tuple.index);

                    var formattedDescription = tuple.variables.Replace(ResolvedDescription);
                    var formattedName = Invariant($"{ResolvedName}[{tuple.index}]");
                    var field = ToField(module, formattedName, formattedDescription, offset);
                    yield return field;
                    offset += gap ?? field.Size;
                }
            }
        }

        // TODO: Maybe validate that this isn't repeated?
        internal IField ToFieldForOverlay(ModuleJson module, ModuleOffset offset) =>
            ToField(module, AssertNotNull(ResolvedName), AssertNotNull(ResolvedDescription), offset);

        private IField ToField(ModuleJson module, string name, string description, ModuleOffset offset)
        {
            Condition?.Validate(name);
            return Type switch
            {
                "boolean" => new BooleanField(BuildCommon(1)),
                "boolean32" => new BooleanField(BuildCommon(4)),
                "placeholder8" => new PlaceholderField(BuildCommon(1)),
                "placeholder16" => new PlaceholderField(BuildCommon(2)),
                "placeholder32" => new PlaceholderField(BuildCommon(4)),
                "enum" => BuildEnumField(1),
                "enum16" => BuildEnumField(2),
                "enum32" => BuildEnumField(4),
                "instrument" => new InstrumentField(BuildCommon(4), ModuleOffset.FromDisplayValue(ValidateNotNull(BankOffset, nameof(BankOffset)).Value)),
                "midi32" => new NumericField(BuildCommon(4), 0, 128, 0, null, null, null, null, (128, "Off")),
                "musicalNote" => new EnumField(BuildCommon(4), MusicalNoteValues, 0, 0),
                "overlay" => BuildOverlay(),
                "range8" => BuildNumericField(1),
                "range16" => BuildNumericField(2),
                "range32" => BuildNumericField(4),
                "string" => BuildStringField(1),
                "string16" => BuildStringField(2),
                "volume32" => new NumericField(BuildCommon(4), -601, 60, 0, 10, null, 0, "dB", (-601, "-INF")),
                _ => throw new InvalidOperationException($"Invalid field type: '{Type}'")
            };

            EnumField BuildEnumField(int size) =>
                new EnumField(BuildCommon(size), ValidateNotNull(Values, nameof(Values)).AsReadOnly(), Min ?? 0, GetDefaultValue());

            StringField BuildStringField(int bytesPerChar)
            {
                var length = ValidateNotNull(Length, nameof(Length));
                var common = BuildCommon(length * bytesPerChar);
                return new StringField(common, length);
            }

            NumericField BuildNumericField(int size)
            {
                var min = ValidateNotNull(Min, nameof(Min));
                var max = ValidateNotNull(Max, nameof(Max));
                Validate(max >= 0, $"Unexpected all-negative field: {name}");
                return new NumericField(BuildCommon(size),
                    min, max, GetDefaultValue(),
                    Divisor, Multiplier, ValueOffset, Suffix,
                    Off == null ? default((int, string)?) : (Off.Value, OffLabel));
            }

            OverlayField BuildOverlay()
            {
                ValidateNotNull(Overlay, nameof(Overlay));

                var fieldsBySwitchValue = Overlay.FieldLists
                    .ToDictionary(pair => pair.Key, pair => ConvertFieldList(pair.Value))
                    .AsReadOnly();
                return new OverlayField(BuildCommon(Overlay.FieldCount * Overlay.FieldSize), Overlay.FieldCount, AssertNotNull(Overlay.SwitchPath), fieldsBySwitchValue);

                OverlayField.FieldList ConvertFieldList(DynamicOverlayJson.OverlaidFieldListJson fieldList)
                {
                    ModuleOffset offsetWithinOverlay = offset;
                    List<IField> ret = new List<IField>();
                    foreach (var fieldJson in fieldList.Fields)
                    {
                        fieldJson.ValidateJson(module);
                        var field = fieldJson.ToFieldForOverlay(module, offsetWithinOverlay);
                        ret.Add(field);
                        offsetWithinOverlay += field.Size;
                        Validate(
                            field.Size == Overlay.FieldSize,
                            "Field {0} in overlay list {1} has inappropriate size",
                            field.Name, fieldList.Description);
                    }
                    return new OverlayField.FieldList(fieldList.Description, ret.Where(field => !(field is PlaceholderField)).ToReadOnlyList());
                }
            }


            FieldBase.Parameters BuildCommon(int size) => new FieldBase.Parameters(name, description, offset, size, Condition?.ToCondition());

            // The default is:
            // - Default if specified
            // - 0 if that's valid
            // - min otherwise
            // Assumption: we never have all-negative fields.
            int GetDefaultValue() => Default ?? Math.Max(Min ?? 0, 0);
        }
    }
}
