// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Data.Fields
{
    public class InstrumentField : NumericFieldBase, IPrimitiveField
    {
        public InstrumentField(FieldPath path, ModuleAddress address, int size, string description, FieldCondition? condition)
            // We don't know how many instruments there will actually be, but we'll validate in GetInstrument.
            : base(path, address, size, description, condition, 0, int.MaxValue)
        {
        }

        public override string GetText(Module module)
        {
            var instrument = GetInstrument(module);
            return $"{instrument.Name} ({instrument.Group.Description})";
        }

        public Instrument GetInstrument(Module module)
        {
            int index = GetRawValue(module.Data);
            if (index >= module.Schema.InstrumentsById.Count)
            {
                throw new InvalidOperationException($"Invalid instrument index {index}. The schema specifies {module.Schema.InstrumentsById.Count} instruments.");
            }
            return module.Schema.InstrumentsById[index];
        }
    }
}
