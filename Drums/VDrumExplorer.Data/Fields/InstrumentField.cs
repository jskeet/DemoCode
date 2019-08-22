// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Linq;

namespace VDrumExplorer.Data.Fields
{
    public class InstrumentField : NumericFieldBase, IPrimitiveField
    {
        internal InstrumentField(FieldBase.Parameters common)
            // Note: although common.Schema isn't fully populated yet, the instruments will be valid by this point.
            : base(common, 0, common.Schema.InstrumentsById.Count)
        {
        }

        public override string GetText(ModuleData data)
        {
            var instrument = GetInstrument(data);
            return $"{instrument.Name} ({instrument.Group.Description})";
        }

        public Instrument GetInstrument(ModuleData data) => Schema.InstrumentsById[GetRawValue(data)];

        public override bool TrySetText(ModuleData data, string text)
        {
            var instrument = Schema.Instruments.FirstOrDefault(inst => inst.Name == text);
            if (instrument != null)
            {
                SetInstrument(data, instrument);
                return false;
            }
            return true;
        }

        public void SetInstrument(ModuleData data, Instrument instrument) => SetRawValue(data, instrument.Id);
    }
}
