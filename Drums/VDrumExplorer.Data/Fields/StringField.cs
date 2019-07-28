using System;
using System.Text;

namespace VDrumExplorer.Data.Fields
{
    public class StringField : FieldBase, IPrimitiveField
    {
        public StringField(FieldPath path, ModuleAddress address, int size, string description, FieldCondition? condition)
            : base(path, address, size, description, condition)
        {
        }

        public string GetText(ModuleData data)
        {
            byte[] bytes = data.GetData(Address, Size);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
