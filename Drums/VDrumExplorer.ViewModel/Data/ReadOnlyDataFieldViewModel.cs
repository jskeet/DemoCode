// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.ComponentModel;
using VDrumExplorer.Model.Data.Fields;

namespace VDrumExplorer.ViewModel.Data
{
    public class ReadOnlyDataFieldViewModel : DataFieldViewModel
    {
        public ReadOnlyDataFieldViewModel(IDataField model) : base(model)
        {
        }

        protected override void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e) =>
            RaisePropertyChanged(nameof(FormattedText));

        public string FormattedText => Model.FormattedText;
    }
}
