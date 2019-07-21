using System.Text;

namespace VDrumExplorer.Models.Fields
{
    public class StringField : FieldBase, IPrimitiveField
    {
        public StringField(string description, string path, int address, int size) : base(description, path, address, size)
        {
        }

        public string GetText(ModuleData data)
        {
            throw new System.NotImplementedException();
        }
    }
}
