// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Data.Fields
{
    public class TempoDataField : DataFieldBase<TempoField>
    {
        // Visible for tools that find raw values more convenient,
        // e.g. CheckMfxDefaultsCommand.
        internal BooleanDataField SwitchDataField { get; }
        internal NumericDataField NumericDataField { get; }
        internal EnumDataField MusicalNoteDataField { get; }

        internal TempoDataField(TempoField field) : base(field)
        {
            SwitchDataField = new BooleanDataField(field.SwitchField);
            NumericDataField = new NumericDataField(field.NumericField);
            MusicalNoteDataField = new EnumDataField(field.MusicalNoteField);
            SwitchDataField.PropertyChanged += HandleSwitchPropertyChanged;
            NumericDataField.PropertyChanged += HandleNumericPropertyChanged;
            MusicalNoteDataField.PropertyChanged += HandleMusicalNotePropertyChanged;
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

        internal override IEnumerable<DataValidationError> Load(DataSegment segment) =>
            SwitchDataField.Load(segment)
                .Concat(NumericDataField.Load(segment))
                .Concat(MusicalNoteDataField.Load(segment));

        internal override void Save(DataSegment segment)
        {
            // While editing, we can maintain the idea of "what would the note field be" while
            // TempoSync is false, and vice versa. When saving to a data segment, we canonicalize
            // the result so that the data is fully predictable from the effective value.
            SwitchDataField.Save(segment);
            if (TempoSync)
            {
                MusicalNoteDataField.Save(segment);
                NumericDataField.SaveDefault(segment);
            }
            else
            {
                NumericDataField.Save(segment);
                MusicalNoteDataField.SaveDefault(segment);
            }
        }

        public bool TempoSync
        {
            get => SwitchDataField.Value;
            set => SwitchDataField.Value = value;
        }

        public int RawNumericValue
        {
            get => NumericDataField.RawValue;
            set => NumericDataField.RawValue = value;
        }

        public string MusicalNote
        {
            get => MusicalNoteDataField.Value;
            set => MusicalNoteDataField.Value = value;
        }

        private const string TempoSyncFormatPrefix = "Tempo sync: ";
        private const string FixedFormatPrefix = "Fixed: ";

        public string NumericFormattedText => NumericDataField.FormattedText;
        public override string FormattedText => TempoSync ? TempoSyncFormatPrefix + MusicalNote : FixedFormatPrefix + NumericDataField.FormattedText;

        public override bool TrySetFormattedText(string text)
        {
            if (text.StartsWith(TempoSyncFormatPrefix, StringComparison.Ordinal))
            {
                text = text.Substring(TempoSyncFormatPrefix.Length);
                if (MusicalNoteDataField.TrySetFormattedText(text))
                {
                    TempoSync = true;
                    return true;
                }
            }
            else if (text.StartsWith(FixedFormatPrefix, StringComparison.Ordinal))
            {
                text = text.Substring(FixedFormatPrefix.Length);
                if (NumericDataField.TrySetFormattedText(text))
                {
                    TempoSync = false;
                    return true;
                }
            }
            return false;
        }

        public override void Reset()
        {
            SwitchDataField.Reset();
            NumericDataField.Reset();
            MusicalNoteDataField.Reset();
        }
    }
}
