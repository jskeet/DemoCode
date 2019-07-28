using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public class BooleanField : EnumField
    {
        private static readonly IReadOnlyList<string> OffOnValues = new List<string> { "Off", "On" }.AsReadOnly();

        public BooleanField(FieldPath path, ModuleAddress address, int size, string description, FieldCondition? condition)
            : base(path, address, size, description, condition, OffOnValues)
        {
        }

        public bool GetValue(ModuleData data) => GetRawValue(data) == 1;
    }
}
