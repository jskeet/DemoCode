﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Data.Fields;

namespace VDrumExplorer.ViewModel.Data
{
    public abstract class DataFieldViewModel : ViewModelBase<IDataField>
    {
        public DataFieldViewModel(IDataField model) : base(model)
        {
        }

        public string Description => Model.SchemaField.Description;

        internal static DataFieldViewModel CreateViewModel(IDataField field, bool readOnly) =>
            readOnly
            ? (DataFieldViewModel) new ReadOnlyDataFieldViewModel(field)
            : field switch
            {
                BooleanDataField model => new EditableBooleanDataFieldViewModel(model),
                EnumDataField model => new EditableEnumDataFieldViewModel(model),
                InstrumentDataField model => new EditableInstrumentDataFieldViewModel(model),
                NumericDataField model => new EditableNumericDataFieldViewModel(model),
                StringDataField model => new EditableStringDataFieldViewModel(model),
                TempoDataField model => new EditableTempoDataFieldViewModel(model),
                _ => throw new ArgumentException($"Can't create editable field of type {field?.GetType()}")
            };
    }

    public abstract class DataFieldViewModel<TModel> : DataFieldViewModel where TModel : IDataField
    {
        public new TModel Model => (TModel) base.Model;

        public DataFieldViewModel(TModel model) : base(model)
        {
        }
    }
}
