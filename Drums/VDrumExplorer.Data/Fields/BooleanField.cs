namespace VDrumExplorer.Data.Fields
{
    public class BooleanField : FieldBase, IPrimitiveField
    {
        public BooleanField(FieldPath path, ModuleAddress address, int size, string description)
            : base(path, address, size, description)
        {
        }

        public string GetText(ModuleData data) => GetRawValue(data) == 0 ? "Off" : "On";
    }
}
