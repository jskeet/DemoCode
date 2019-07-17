using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Models.Fields
{
    class BooleanValue : FieldBase
    {
        public override FieldValue ParseSysExData(byte[] data)
        {
            string text = data[Address] == 0 ? "Off"
                : data[Address] == 1 ? "On"
                : throw new InvalidOperationException($"Invalid Boolean value: {data[Address]}");
            return new FieldValue(text);
        }
    }
}
