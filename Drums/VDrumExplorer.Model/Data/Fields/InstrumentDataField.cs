// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    // TODO: Subscribe to changes in the bank field
    public class InstrumentDataField : DataFieldBase<InstrumentField>
    {
        private readonly EnumDataField bankField;

        public InstrumentDataField(FieldContainerData context, InstrumentField field) : base(context, field)
        {
            var (bankContainer, bankSchemaField) = context.FieldContainer.ResolveField(SchemaField.BankPath);
            bankField = (EnumDataField) context.ModuleData.CreateDataField(bankContainer, bankSchemaField);
            AddFieldMatcher(bankContainer, bankSchemaField);
        }

        protected override void OnDataChanged() => RaisePropertyChange(nameof(Instrument));

        public Instrument Instrument
        {
            get => GetInstrument();
            set => SetInstrument(value);
        }

        private Instrument GetInstrument()
        {
            var schema = Context.FieldContainer.Schema;
            var index = Context.ReadInt32(Offset, Size);
            var bank = bankField.RawValue;
            return bank == 0
                ? schema.PresetInstruments[index]
                : schema.UserSampleInstruments[index];
        }

        private void SetInstrument(Instrument instrument)
        {
            Context.WriteInt32(Offset, Size, instrument.Id);
            bankField.RawValue = instrument.Group is null ? 1 : 0;
        }

        public override string FormattedText => Instrument.Name;
    }
}
