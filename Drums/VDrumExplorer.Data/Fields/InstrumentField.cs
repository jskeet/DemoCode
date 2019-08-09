// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Data.Fields
{
    public class InstrumentField : NumericFieldBase, IPrimitiveField
    {
        internal InstrumentField(FieldBase.Parameters common)
            // We don't know how many instruments there will actually be, but we'll validate in GetInstrument.
            : base(common, 0, int.MaxValue)
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
            if (index >= Schema.InstrumentsById.Count)
            {
                throw new InvalidOperationException($"Invalid instrument index {index}. The schema specifies {Schema.InstrumentsById.Count} instruments.");
            }
            return Schema.InstrumentsById[index];
        }
    }
}
