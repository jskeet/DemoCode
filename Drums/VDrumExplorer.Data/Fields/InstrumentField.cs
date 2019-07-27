namespace VDrumExplorer.Data.Fields
{
    // TODO: Make this a range field? Or an enum?
    public class InstrumentField : FieldBase, IPrimitiveField
    {
        public InstrumentField(FieldPath path, ModuleAddress address, int size, string description)
            : base(path, address, size, description)
        {
        }

        public string GetText(ModuleData data)
        {
            var instrument = GetInstrument(data);
            return $"{instrument.Name} ({instrument.Group.Description})";
        }

        public Instrument GetInstrument(ModuleData data) =>
            data.Schema.InstrumentsById[GetRawValue(data)];
    }
}
