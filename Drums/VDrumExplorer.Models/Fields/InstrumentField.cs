using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Models.Fields
{
    // TODO: Make this a range field? Or an enum?
    public class InstrumentField : FieldBase, IPrimitiveField
    {
        public InstrumentField(string description, string path, int address, int size)
            : base(description, path, address, size)
        {
        }

        public string GetText(ModuleData data) => GetInstrument(data).Name;

        public Instrument GetInstrument(ModuleData data) =>
            data.ModuleFields.InstrumentsById[GetRawValue(data)];
    }
}
