// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using VDrumExplorer.Model.Data.Logical;

namespace VDrumExplorer.ViewModel.Data
{
    public class ListDataNodeDetailViewModel : ViewModelBase<ListDataNodeDetail>, IDataNodeDetailViewModel
    {
        public string Description { get; }
        public IReadOnlyList<DataFieldFormattableString> Items { get; }

        public ListDataNodeDetailViewModel(ListDataNodeDetail model) : base(model)
        {
            Description = model.Description;
            Items = model.Items;
        }
    }
}
