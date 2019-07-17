using System.Text;

namespace VDrumExplorer.Models.Fields
{
    public class StringField : FieldBase
    {
        public int Length { get; set; }

        public override FieldValue ParseSysExData(byte[] data)
        {
            string text = Encoding.ASCII.GetString(data, Address, Length);
            return new FieldValue(text);
        }
    }
}
