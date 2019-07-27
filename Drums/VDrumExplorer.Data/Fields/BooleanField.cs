using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public class BooleanField : EnumField
    {
        private static readonly IReadOnlyList<string> values = new List<string> { "Off", "On" }.AsReadOnly();

        public BooleanField(FieldPath path, ModuleAddress address, int size, string description)
            : base(path, address, size, description, values)
        {
        }

        public bool GetValue(ModuleData data) => GetRawValue(data) == 1;
    }
}
