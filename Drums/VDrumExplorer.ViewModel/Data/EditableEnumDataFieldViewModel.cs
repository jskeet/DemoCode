// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.ComponentModel;
using VDrumExplorer.Model.Data.Fields;

namespace VDrumExplorer.ViewModel.Data
{
    public class EditableEnumDataFieldViewModel : DataFieldViewModel<EnumDataField>
    {
        public EditableEnumDataFieldViewModel(EnumDataField model) : base(model)
        {
        }

        protected override void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e) =>
            RaisePropertyChanged(nameof(Value));

        public IReadOnlyList<string> ValidValues => Model.SchemaField.Values;

        public string Value
        {
            get => Model.Value;
            set => Model.Value = value;
        }
    }
}
