// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Data.Logical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.Data
{
    public class FieldContainerDataNodeDetailViewModel : ViewModelBase<FieldContainerDataNodeDetail>, IDataNodeDetailViewModel
    {
        public string Description { get; }

        private readonly DataExplorerViewModel windowViewModel;

        // TODO: ObservableCollection? Might allow for smoother "enter editing mode" experience.
        private IReadOnlyList<DataFieldViewModel> fields;
        public IReadOnlyList<DataFieldViewModel> Fields
        {
            get => fields;
            set => SetProperty(ref fields, value);
        }

        public FieldContainerDataNodeDetailViewModel(FieldContainerDataNodeDetail model, DataExplorerViewModel windowViewModel) : base(model)
        {
            Description = model.Description;
            this.windowViewModel = windowViewModel;
            fields = GenerateFields(model.Fields).ToReadOnlyList();
        }

        private void RefreshFields() => Fields = GenerateFields(Model.Fields).ToReadOnlyList();

        protected override void OnPropertyChangedHasSubscribers()
        {
            windowViewModel.PropertyChanged += Parent_PropertyChanged;
            foreach (var field in Model.Fields.OfType<OverlayDataField>())
            {
                field.PropertyChanged += FieldListChanged;
            }
        }

        private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(windowViewModel.ReadOnly))
            {
                RefreshFields();
            }
        }

        protected override void OnPropertyChangedHasNoSubscribers()
        {
            windowViewModel.PropertyChanged -= Parent_PropertyChanged;
            foreach (var field in Model.Fields.OfType<OverlayDataField>())
            {
                field.PropertyChanged -= FieldListChanged;
            }
        }

        private void FieldListChanged(object sender, PropertyChangedEventArgs e) => RefreshFields();

        private IEnumerable<DataFieldViewModel> GenerateFields(IEnumerable<IDataField> fields)
        {
            foreach (var field in fields)
            {
                if (field is OverlayDataField odf)
                {
                    foreach (var vm in GenerateFields(odf.CurrentFieldList.Fields))
                    {
                        yield return vm;
                    }
                }
                else
                {
                    yield return DataFieldViewModel.CreateViewModel(field, windowViewModel.ReadOnly);
                }
            }
        }
    }
}
