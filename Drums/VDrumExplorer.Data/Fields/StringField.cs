using System;
using System.Text;

namespace VDrumExplorer.Data.Fields
{
    public class StringField : FieldBase, IPrimitiveField
    {
        public StringField(FieldPath path, ModuleAddress address, int size, string description)
            : base(path, address, size, description)
        {
        }

        public string GetText(ModuleData data)
        {
            byte[] bytes = data.GetData(Address, Size);
            return Encoding.ASCII.GetString(bytes);
        }

        internal override int GetRawValue(ModuleData data) =>
            throw new InvalidOperationException($"String fields do not have a raw numeric value");
    }
}
