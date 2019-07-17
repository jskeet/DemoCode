using System.Collections.Generic;

namespace VDrumExplorer.Models.Fields
{
    public class EnumField : FieldBase
    {
        public List<string> Values { get; set; }

        public override FieldValue ParseSysExData(byte[] data) =>
            new FieldValue(Values[data[Address]]);
    }
}
