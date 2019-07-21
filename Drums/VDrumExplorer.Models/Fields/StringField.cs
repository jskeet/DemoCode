using System;
using System.Text;

namespace VDrumExplorer.Models.Fields
{
    public class StringField : FieldBase, IPrimitiveField
    {
        public StringField(string description, string path, ModuleAddress address, int size)
            : base(description, path, address, size)
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
