// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Logical;

namespace VDrumExplorer.ViewModel.LogicalSchema
{
    public sealed class ListNodeDetailViewModel : NodeDetailViewModel<ListNodeDetail>
    {
        public ListNodeDetailViewModel(ListNodeDetail model) : base(model)
        {
            Items = model.Items.Select(item => new FieldFormattableStringViewModel(item)).ToList().AsReadOnly();
        }

        public IReadOnlyList<FieldFormattableStringViewModel> Items { get; }
    }
}
