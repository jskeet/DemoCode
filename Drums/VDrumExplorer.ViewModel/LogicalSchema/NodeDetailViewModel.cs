// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Schema.Logical;

namespace VDrumExplorer.ViewModel.LogicalSchema
{
    public abstract class NodeDetailViewModel : ViewModelBase<INodeDetail>
    {
        public NodeDetailViewModel(INodeDetail model) : base(model)
        {
        }

        public string Description => Model.Description;

        public static NodeDetailViewModel Create(INodeDetail detail) =>
            detail switch
            {
                ListNodeDetail model => new ListNodeDetailViewModel(model),
                FieldContainerNodeDetail model => new FieldContainerNodeDetailViewModel(model),
                _ => throw new ArgumentException($"Unknown detail type: {detail?.GetType()}")
            };
    }

    public abstract class NodeDetailViewModel<TModel> : NodeDetailViewModel where TModel : INodeDetail
    {
        public new TModel Model => (TModel) base.Model;

        public NodeDetailViewModel(TModel model) : base(model)
        {
        }
    }
}
