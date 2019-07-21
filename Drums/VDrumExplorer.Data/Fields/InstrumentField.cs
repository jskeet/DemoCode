namespace VDrumExplorer.Models.Fields
{
    // TODO: Make this a range field? Or an enum?
    public class InstrumentField : FieldBase, IPrimitiveField
    {
        public InstrumentField(string description, string path, ModuleAddress address, int size)
            : base(description, path, address, size)
        {
        }

        public string GetText(ModuleData data)
        {
            var instrument = GetInstrument(data);
            return $"{instrument.Name} ({instrument.Group.Description})";
        }

        public Instrument GetInstrument(ModuleData data) =>
            data.ModuleFields.InstrumentsById[GetRawValue(data)];
    }
}
