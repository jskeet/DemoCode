using System.Collections.Generic;

namespace VDrumExplorer.Models.Fields
{
    public sealed class EnumField : FieldBase, IPrimitiveField
    {
        public IReadOnlyList<string> Values { get; }

        public EnumField(string description, string path, int address, int size, IReadOnlyList<string> values)
            : base(description, path, address, size) =>
            Values = values;

        public string GetText(ModuleData data) => Values[GetRawValue(data)];
    }
}
