// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using VDrumExplorer.Model.Data.Fields;

namespace VDrumExplorer.ViewModel.Data
{
    public class EditableTempoDataFieldViewModel : DataFieldViewModel<TempoDataField>
    {
        public EditableTempoDataFieldViewModel(TempoDataField model) : base(model)
        {
        }

        protected override void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(TempoSync));
            RaisePropertyChanged(nameof(NotTempoSync));
            RaisePropertyChanged(nameof(NumericValue));
            RaisePropertyChanged(nameof(MusicalNote));
            RaisePropertyChanged(nameof(FormattedText));
            RaisePropertyChanged(nameof(NumericFormattedText));
        }

        public IReadOnlyList<string> ValidMusicalNoteValues => Model.SchemaField.MusicalNoteField.Values;
        public int MinNumericValue => Model.SchemaField.NumericField.Min;
        public int MaxNumericValue => Model.SchemaField.NumericField.Max;
        public int LargeNumericChange => Math.Max((MaxNumericValue - MinNumericValue) / 10, 1);

        public bool TempoSync
        {
            get => Model.TempoSync;
            set => Model.TempoSync = value;
        }

        public bool NotTempoSync => !TempoSync;

        public int NumericValue
        {
            get => Model.RawNumericValue;
            set => Model.RawNumericValue = value;
        }

        public string MusicalNote
        {
            get => Model.MusicalNote;
            set => Model.MusicalNote = value;
        }

        public string FormattedText => Model.FormattedText;

        public string NumericFormattedText => Model.NumericFormattedText;
    }
}
