using System;

namespace VDrumExplorer.Data.Fields
{
    // TODO: Make this a range field? Or an enum?
    public class InstrumentField : NumericFieldBase, IPrimitiveField
    {
        public InstrumentField(FieldPath path, ModuleAddress address, int size, string description, FieldCondition? condition)
            // We don't know how many instruments there will actually be, but we'll validate in GetInstrument.
            : base(path, address, size, description, condition, 0, int.MaxValue)
        {
        }

        public override string GetText(ModuleData data)
        {
            var instrument = GetInstrument(data);
            return $"{instrument.Name} ({instrument.Group.Description})";
        }

        public Instrument GetInstrument(ModuleData data)
        {
            int index = GetRawValue(data);
            if (index >= data.Schema.InstrumentsById.Count)
            {
                throw new InvalidOperationException($"Invalid instrument index {index}. The schema specifies {data.Schema.InstrumentsById.Count} instruments.");
            }
            return data.Schema.InstrumentsById[index];
        }
    }
}
