using Newtonsoft.Json.Linq;
using VDrumExplorer.Models.Fields;

namespace VDrumExplorer.Models.TD17
{
    public class TD17InstrumentFields
    {
        private readonly InstrumentFields instrumentFields;
        
        public TD17KitFields Kit { get; }
        public int InstrumentNumber => instrumentFields.InstrumentNumber;            

        public FieldSet Common => instrumentFields.FieldSets[0];
        public FieldSet Main => instrumentFields.FieldSets[1];
        public FieldSet Sub => instrumentFields.FieldSets[2];
        public FieldSet VEditMain => instrumentFields.FieldSets[3];
        public FieldSet VEditSub => instrumentFields.FieldSets[4];

        internal TD17InstrumentFields(TD17KitFields kit, InstrumentFields instrumentFields)
        {
            Kit = kit;
            this.instrumentFields = instrumentFields;
        }
    }
}
