// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Fields;
using VDrumExplorer.ViewModel;

namespace VDrumExplorer.ViewModels.Fields
{
    public abstract class FieldViewModel : ViewModelBase<IField>
    {
        protected FieldViewModel(IField model) : base(model)
        {
        }
    }

    public abstract class FieldViewModel<TModel> : FieldViewModel where TModel : IField
    {
        public new TModel Model => (TModel) base.Model;

        protected FieldViewModel(TModel model) : base(model)
        {
        }
    }
}
