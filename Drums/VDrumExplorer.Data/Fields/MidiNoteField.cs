// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Fields
{
    public class MidiNoteField : NumericField
    {
        internal MidiNoteField(Parameters common) : base(common,
            min: 0, max: 128, divisor: null, multiplier: null, valueOffset: null, suffix: null,
            customValueFormatting: (128, "Off"))
        {
        }

        public int? GetMidiNote(ModuleData data)
        {
            var note = GetRawValue(data);
            return note == 128 ? default(int?) : note;
        }            
    }
}
