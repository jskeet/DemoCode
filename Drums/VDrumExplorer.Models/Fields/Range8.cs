using System;
using System.Globalization;

namespace VDrumExplorer.Models.Fields
{
    public class Range8 : FieldBase
    {
        /// <summary>
        /// The maximum value for this field, inclusive.
        /// </summary>
        public int? Max { get; }

        /// <summary>
        /// The minimum value for this field, inclusive.
        /// </summary>
        public int? Min { get; }

        /// <summary>
        /// The value for this field representing "off".
        /// </summary>
        public int? Off { get; }

        public override FieldValue ParseSysExData(byte[] data)
        {
            int value = data[Address];
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
