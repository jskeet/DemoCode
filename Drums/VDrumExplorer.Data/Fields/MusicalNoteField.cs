using System;

namespace VDrumExplorer.Data.Fields
{
    // TODO: Make this a range field? Or an enum?
    public class MusicalNoteField : FieldBase, IPrimitiveField
    {
        public MusicalNoteField(FieldPath path, ModuleAddress address, int size, string description)
            : base(path, address, size, description)
        {
        }

        public string GetText(ModuleData data)
        {
            return "FIXME";
        }
    }
}
