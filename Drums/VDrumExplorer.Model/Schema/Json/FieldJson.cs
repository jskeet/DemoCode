// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

#nullable disable warnings

using Newtonsoft.Json.Linq;
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
        /// The values for enum fields that don't have contiguous values. Each array should consist
        /// of a number and then a string.
        /// </summary>
        public List<JArray>? ValuesByNumber { get; set; }

        /// <summary>
        /// For instrument fields only, the offset of the bank switch field, in the same container.
        /// </summary>
        public HexInt32? BankOffset { get; set; }

        /// <summary>
        /// For "overlay" fields only, the details of the dynamic overlay.
        /// </summary>
        public DynamicOverlayJson? Overlay { get; set; }

        public override string ToString() => Description ?? Name ?? "(No description)";

        internal IEnumerable<FieldBase> ToFields(ModuleJson module, ModuleOffset offset)
        {
            AssertNotNull(ResolvedName);
            AssertNotNull(ResolvedDescription);

            if (Repeat is null)
            {
                yield return ToField(module, ResolvedName, ResolvedDescription, offset);
            }
            else
            {
                AssertNotNull(Repeat.Items);
                var variables = SchemaVariables.Empty;
                int? gap = Repeat.Gap?.Value;
                foreach (var tuple in module.GetRepeatSequence(Repeat.Items, variables))
                {
                    var itemVariables = variables.WithVariable(Repeat.IndexVariable, tuple.index, Repeat.IndexTemplate);

                    var formattedDescription = tuple.variables.Replace(ResolvedDescription);
                    var formattedName = Invariant($"{ResolvedName}[{tuple.index}]");
                    var field = ToField(module, formattedName, formattedDescription, offset);
                    yield return field;
                    offset += gap ?? field.Size;
                }
            }
        }

        internal FieldBase ToFieldForOverlay(ModuleJson module, ModuleOffset offset)
        {
            AssertNotNull(ResolvedName);
            Validate(Repeat is null, "Repeated fields are not valid in overlays");
            return ToField(module, AssertNotNull(ResolvedName), AssertNotNull(ResolvedDescription), offset);
        }

        internal FieldBase ToField(ModuleJson module, string name, string description, ModuleOffset offset)
        {
            return Type switch
            {
                "boolean" => new BooleanField(null, BuildCommon(1)),
                "boolean32" => new BooleanField(null, BuildCommon(4)),
                string ph when ph.StartsWith("placeholder") => new PlaceholderField(null, BuildCommon(int.Parse(ph.Substring("placeholder".Length)) / 8)),
                "enum" => BuildEnumField(1),
                "enum16" => BuildEnumField(2),
                "enum24" => BuildEnumField(3),
                "enum32" => BuildEnumField(4),
                "instrument" => new InstrumentField(null, BuildCommon(4), ModuleOffset.FromDisplayValue(ValidateNotNull(BankOffset, nameof(BankOffset)).Value)),
                "midi32" => new NumericField(null, BuildCommon(4), 0, 128, Default ?? 0, null, null, null, null, (128, "Off")),
                "overlay" => BuildOverlay(),
                "range8" => BuildNumericField(1, 0, 127),
                "range16" => BuildNumericField(2, -128, 127),
                "range32" => BuildNumericField(4, short.MinValue, short.MaxValue),
                "string" => BuildStringField(1),
                "string16" => BuildStringField(2),
                "tempo" => BuildTempoField(),
                "volume32" => new NumericField(null, BuildCommon(4), -601, 60, 0, 10, null, 0, "dB", (-601, "-INF")),
                _ => throw new InvalidOperationException($"Invalid field type: '{Type}'")
            };

            EnumField BuildEnumField(int size)
            {
                IReadOnlyList<(int number, string value)> numberValuePairs;
                if (Values is object)
                {
                    int min = Min ?? 0;
                    ValidateNull(ValuesByNumber, nameof(ValuesByNumber), nameof(Values));
                    numberValuePairs = Values
                        .Select((value, index) => (index + min, value))
                        .ToReadOnlyList();
                }
                else
                {
                    ValidateNotNull(ValuesByNumber, nameof(ValuesByNumber));
                    Validate(ValuesByNumber.All(array => array.Count == 2 && array[0] is JToken { Type: JTokenType.Integer } && array[1] is JToken { Type: JTokenType.String }),
                        "All arrays in {0} must be [number, value] pairs", nameof(ValuesByNumber));
                    numberValuePairs = ValuesByNumber.ToReadOnlyList(array => ((int) array[0], (string) array[1]));
                }
                return new EnumField(null, BuildCommon(size), numberValuePairs, GetDefaultValue());
            }

            StringField BuildStringField(int bytesPerChar)
            {
                var length = ValidateNotNull(Length, nameof(Length));
                var common = BuildCommon(length * bytesPerChar);
                return new StringField(null, common, length);
            }

            TempoField BuildTempoField()
            {
                var min = ValidateNotNull(Min, nameof(Min));
                var max = ValidateNotNull(Max, nameof(Max));
                Validate(max >= 0, $"Unexpected all-negative field: {name}");
                return new TempoField(null, BuildCommon(12),
                    min, max, GetDefaultValue(),
                    Divisor, Multiplier, ValueOffset, Suffix,
                    Off == null ? default((int, string)?) : (Off.Value, OffLabel));
            }

            NumericField BuildNumericField(int size, int minMin, int maxMax)
            {
                var min = ValidateNotNull(Min, nameof(Min));
                var max = ValidateNotNull(Max, nameof(Max));
                Validate(max >= 0, $"Unexpected all-negative field: {name}");
                Validate(min >= minMin, $"Field {name} has min value {min}, below {minMin}");
                Validate(max <= maxMax, $"Field {name} has max value {max}, above {maxMax}");
                return new NumericField(null, BuildCommon(size),
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
                return new OverlayField(null, BuildCommon(Overlay.FieldCount * Overlay.FieldSize), Overlay.FieldCount, AssertNotNull(Overlay.SwitchPath), fieldsBySwitchValue);

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
                            // The field might actually encompass multiple underlying fields - e.g. for tempo fields.
                            (field.Size % Overlay.FieldSize) == 0,
                            "Field {0} in overlay list {1} has inappropriate size",
                            field.Name, fieldList.Description);
                    }
                    return new OverlayField.FieldList(fieldList.Description, ret.Where(field => !(field is PlaceholderField)).ToReadOnlyList());
                }
            }


            FieldBase.FieldParameters BuildCommon(int size) => new FieldBase.FieldParameters(name, description, offset, size);

            // The default is:
            // - Default if specified
            // - 0 if that's valid
            // - min otherwise
            // Assumption: we never have all-negative fields.
            int GetDefaultValue() => Default ?? Math.Max(Min ?? 0, 0);
        }
    }
}
