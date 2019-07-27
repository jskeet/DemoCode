using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public sealed class EnumField : FieldBase, IPrimitiveField
    {
        public IReadOnlyList<string> Values { get; }

        public EnumField(FieldPath path, ModuleAddress address, int size, string description, IReadOnlyList<string> values)
            : base(path, address, size, description) =>
            Values = values;

        public string GetText(ModuleData data) => Values[GetRawValue(data)];
    }
}
