using System;
using System.Globalization;

namespace VDrumExplorer.Models.Fields
{
    public class Range16 : FieldBase
    {
        /// <summary>
        /// The maximum value for this field, inclusive.
        /// </summary>
        public int? Max { get; set; }

        /// <summary>
        /// The minimum value for this field, inclusive.
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        /// The value for this field representing "off".
        /// </summary>
        public int? Off { get; set; }

        public override FieldValue ParseSysExData(byte[] data)
        {
            int value = GetInt16Value(data);
            if (value == Off)
            {
                return new FieldValue("Off");
            }
            if (value > Max || value < Min)
            {
                throw new InvalidOperationException($"Invalid range value: {value}");
            }
            return new FieldValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
