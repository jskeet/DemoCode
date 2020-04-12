// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

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

        private IReadOnlyList<SimpleDataFieldViewModel> fields;

        public IReadOnlyList<SimpleDataFieldViewModel> Fields
        {
            get => fields;
            set => SetProperty(ref fields, value);
        }

        public FieldContainerDataNodeDetailViewModel(FieldContainerDataNodeDetail model) : base(model)
        {
            Description = model.Description;
            fields = GenerateFields(model.Fields).ToReadOnlyList();
        }

        protected override void OnPropertyChangedHasSubscribers()
        {
            foreach (var field in Model.Fields.OfType<OverlayDataField>())
            {
                field.PropertyChanged += FieldListChanged;
            }
        }

        protected override void OnPropertyChangedHasNoSubscribers()
        {
            foreach (var field in Model.Fields.OfType<OverlayDataField>())
            {
                field.PropertyChanged -= FieldListChanged;
            }
        }

        private void FieldListChanged(object sender, PropertyChangedEventArgs e) =>
            Fields = GenerateFields(Model.Fields).ToReadOnlyList();

        private static IEnumerable<SimpleDataFieldViewModel> GenerateFields(IEnumerable<IDataField> fields)
        {
            foreach (var field in fields)
            {
                if (field is OverlayDataField odf)
                {
                    foreach (var vm in GenerateFields(odf.GetFieldList().Fields))
                    {
                        yield return vm;
                    }
                }
                else
                {
                    yield return new SimpleDataFieldViewModel(field);
                }
            }
        }
    }
}
