// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    public class InstrumentDataField : DataFieldBase<InstrumentField>
    {
        // TODO: Check whether we actually need this publicly.
        public ModuleSchema Schema { get; }

        internal InstrumentDataField(InstrumentField field, ModuleSchema schema) : base(field)
        {
            // TODO: Use primitive data fields as we do in TempoDataField?
            Schema = schema;
        }

        protected override void RaisePropertyChanges() =>
            throw new InvalidOperationException("Bug in V-Drum Explorer. This type has complex property semantics, and shouldn't use SetProperty.");

        // Value in the main field.
        private int index;
        // Value in the bank field
        private InstrumentBank bank;

        public override void Reset() => Instrument = Schema.PresetInstruments[0];

        internal override IEnumerable<DataValidationError> Load(DataSegment segment)
        {
            var indexValue = segment.ReadInt32(Offset, NumericCodec.Range32);
            var bankValue = SchemaField.BankOffset is null
                ? InstrumentBank.Preset
                : (InstrumentBank) segment.ReadInt32(SchemaField.BankOffset.Value, NumericCodec.Range8);

            if (bankValue == InstrumentBank.Preset && indexValue >= 0 && indexValue < Schema.PresetInstruments.Count)
            {
                Instrument = Schema.PresetInstruments[indexValue];
                return DataValidationError.None;
            }
            else if (bankValue == InstrumentBank.UserSamples && indexValue >= 0 && indexValue < Schema.UserSampleInstruments.Count)
            {
                Instrument = Schema.UserSampleInstruments[indexValue];
                return DataValidationError.None;
            }
            else
            {
                return new[] { new DataValidationError(this, $"Invalid instrument field bank/index combination: {bankValue}/{indexValue}") };
            }
        }

        internal override void Save(DataSegment segment)
        {
            segment.WriteInt32(Offset, NumericCodec.Range32, index);
            if (SchemaField.BankOffset is ModuleOffset offset)
            {
                segment.WriteInt32(offset, NumericCodec.Range8, (int) bank);
            }
        }

        public Instrument Instrument
        {
            get => bank == InstrumentBank.Preset ? Schema.PresetInstruments[index] : Schema.UserSampleInstruments[index];
            set => SetInstrument(value);
        }

        public InstrumentGroup Group
        {
            get => Instrument.Group;
            set
            {
                if (value == Instrument.Group)
                {
                    return;
                }
                SetInstrument(value.Instruments[0]);
            }
        }

        private void SetInstrument(Instrument instrument)
        {
            if (instrument.Id == index && instrument.Bank == bank)
            {
                return;
            }
            bool groupChange = instrument.Group == Group;
            index = instrument.Id;
            bank = instrument.Bank;

            if (groupChange)
            {
                RaisePropertyChanged(nameof(Group));
            }
            RaisePropertyChanged(nameof(Instrument));
        }

        public override string FormattedText => Instrument.Name;

        public override bool TrySetFormattedText(string text)
        {
            var instrument = Schema.PresetInstruments.Concat(Schema.UserSampleInstruments).FirstOrDefault(inst => inst.Name == text);
            if (instrument is not null)
            {
                SetInstrument(instrument);
            }
            return instrument is not null;
        }
    }
}
