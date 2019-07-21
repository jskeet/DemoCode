using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace VDrumExplorer.Models.Json
{
    internal class FieldJson
    {
        /// <summary>
        /// Developer-oriented comment. Has no effect.
        /// </summary>
        public string Comment { get; set; }
        
        public string Description { get; set; }

        public HexString Offset { get; set; }

        /// <summary>
        /// The type of the field. If this begins with "container:" then the
        /// text following the prefix is the container name.
        /// </summary>
        public string Type { get; set; }

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
        public int? Divisor { get; }

        /// <summary>
        /// Amount to multiply the value by, for ranges (e.g. 2 for a 0, 2, 4 etc value).
        /// </summary>
        public int? Multiplier { get; }

        /// <summary>
        /// The suffix to apply, usually a unit e.g. "dB".
        /// </summary>
        public string Suffix { get; }

        /// <summary>
        /// The amount to add to the stored value to get the displayed value. This is applied
        /// before multiplication or division.
        /// </summary>
        public int? ValueOffset { get; set; }

        /// <summary>
        /// The values (from 0 upwards) for enum fields.
        /// </summary>
        public List<string> Values { get; set; }

        /// <summary>
        /// The number of times this field is repeated. May be numeric, or
        /// a variable value such as "$kits" or "$instruments".
        /// </summary>
        public string Repeat { get; set; }

        /// <summary>
        /// The gap between repeated fields (from start to start).
        /// </summary>
        public HexString Gap { get; set; }

        /// <summary>
        /// The details for a DynamicOverlay field.
        /// </summary>
        public DynamicOverlayJson DynamicOverlay { get; set; }

        public int? GetRepeat(ModuleJson moduleJson)
        {
            switch (Repeat)
            {
                case null: return null;
                case "$kits": return moduleJson.Kits;
                case "$instruments": return moduleJson.InstrumentsPerKit;
                default:
                    if (!int.TryParse(Repeat, NumberStyles.None, CultureInfo.InvariantCulture, out var result))
                    {
                        throw new InvalidOperationException($"Invalid repeat value: '{Repeat}'");
                    }
                    return result;
            }
        }

        public override string ToString() => Description;
    }
}
