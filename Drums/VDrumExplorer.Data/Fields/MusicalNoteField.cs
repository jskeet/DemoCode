using System;

namespace VDrumExplorer.Data.Fields
{
    // TODO: Make this a range field? Or an enum?
    public class MusicalNoteField : FieldBase, IPrimitiveField
    {
        public MusicalNoteField(string description, string path, ModuleAddress address, int size)
            : base(description, path, address, size)
        {
        }

        public string GetText(ModuleData data)
        {
            return "FIXME";
        }
    }
}
