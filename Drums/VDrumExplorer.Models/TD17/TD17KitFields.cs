using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDrumExplorer.Models.Fields;

namespace VDrumExplorer.Models.TD17
{
    public class TD17KitFields
    {
        private readonly KitFields kitFields;
        
        public TD17ModuleFields Module { get; }
        public int KitNumber => kitFields.KitNumber;

        public FieldSet Common => kitFields.FieldSets[0];
        public FieldSet Midi => kitFields.FieldSets[1];
        public FieldSet Ambience => kitFields.FieldSets[2];
        public FieldSet MultiFx => kitFields.FieldSets[3];
        public IReadOnlyList<TD17InstrumentFields> Instruments { get; }

        public TD17KitFields(TD17ModuleFields module, KitFields kitFields)
        {
            Module = module;
            this.kitFields = kitFields;
            Instruments = kitFields.Instruments.Select(i => new TD17InstrumentFields(this, i)).ToList().AsReadOnly();
        }
    }
}
