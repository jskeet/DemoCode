using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Data.Fields
{
    public class BooleanField : FieldBase, IPrimitiveField
    {
        public BooleanField(string description, string path, ModuleAddress address, int size)
            : base(description, path, address, size)
        {
        }

        public string GetText(ModuleData data) => GetRawValue(data) == 0 ? "Off" : "On";
    }
}
