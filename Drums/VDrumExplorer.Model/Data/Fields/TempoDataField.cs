// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.ComponentModel;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    public class TempoDataField : DataFieldBase<TempoField>
    {
        private readonly BooleanDataField switchDataField;
        private readonly NumericDataField numericDataField;
        private readonly EnumDataField musicalNoteDataField;

        internal TempoDataField(TempoField field) : base(field)
        {
            switchDataField = new BooleanDataField(field.SwitchField);
            numericDataField = new NumericDataField(field.NumericField);
            musicalNoteDataField = new EnumDataField(field.MusicalNoteField);
        }

        protected override void OnPropertyChangedHasSubscribers()
        {
            switchDataField.PropertyChanged += HandleSwitchPropertyChanged;
            numericDataField.PropertyChanged += HandleNumericPropertyChanged;
            musicalNoteDataField.PropertyChanged += HandleMusicalNotePropertyChanged;
        }

        protected override void OnPropertyChangedHasNoSubscribers()
        {
            switchDataField.PropertyChanged -= HandleSwitchPropertyChanged;
            numericDataField.PropertyChanged -= HandleNumericPropertyChanged;
            musicalNoteDataField.PropertyChanged -= HandleMusicalNotePropertyChanged;
        }

        private void HandleSwitchPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(TempoSync));
            RaisePropertyChanged(nameof(FormattedText));
        }

        private void HandleNumericPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(RawNumericValue));
            RaisePropertyChanged(nameof(NumericFormattedText));
            RaisePropertyChanged(nameof(FormattedText));
        }

        private void HandleMusicalNotePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(MusicalNote));
            RaisePropertyChanged(nameof(FormattedText));
        }

        internal override void Load(DataSegment segment)
        {
            switchDataField.Load(segment);
            numericDataField.Load(segment);
            musicalNoteDataField.Load(segment);
        }

        internal override void Save(DataSegment segment)
        {
            switchDataField.Save(segment);
            numericDataField.Save(segment);
            musicalNoteDataField.Save(segment);
        }

        public bool TempoSync
        {
            get => switchDataField.Value;
            set => switchDataField.Value = value;
        }

        public int RawNumericValue
        {
            get => numericDataField.RawValue;
            set => numericDataField.RawValue = value;
        }

        public string MusicalNote
        {
            get => musicalNoteDataField.Value;
            set => musicalNoteDataField.Value = value;
        }

        public string NumericFormattedText => numericDataField.FormattedText;
        public override string FormattedText => TempoSync ? $"Tempo sync: {MusicalNote}" : $"Fixed: {numericDataField.FormattedText}";

        public override void Reset()
        {
            switchDataField.Reset();
            numericDataField.Reset();
            musicalNoteDataField.Reset();
        }
    }
}
