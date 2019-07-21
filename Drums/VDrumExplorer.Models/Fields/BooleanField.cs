using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Models.Fields
{
    public class BooleanField : FieldBase, IPrimitiveField
    {
        public BooleanField(string description, string path, int address, int size) : base(description, path, address, size)
        {
        }

        public string GetText(ModuleData data)
        {
            throw new NotImplementedException();
        }
    }
}
