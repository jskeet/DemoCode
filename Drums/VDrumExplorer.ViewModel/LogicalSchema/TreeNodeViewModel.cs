// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.LogicalSchema
{
    public class TreeNodeViewModel : ViewModelBase<TreeNode>
    {
        private readonly Lazy<IReadOnlyList<TreeNodeViewModel>> children;
        private readonly Lazy<IReadOnlyList<NodeDetailViewModel>> details;
        private readonly Lazy<IReadOnlyList<KeyValueViewModel>> table;

        public TreeNodeViewModel(TreeNode model) : base(model)
        {
            Format = new FieldFormattableStringViewModel(Model.Format);
            Lazy.Initialize(out children, () => Model.Children.ToReadOnlyList(child => new TreeNodeViewModel(child)));
            Lazy.Initialize(out details, () => Model.Details.ToReadOnlyList(child => NodeDetailViewModel.Create(child)));
            Lazy.Initialize(out table, () => CreateTable().ToReadOnlyList());
        }

        public FieldFormattableStringViewModel Format { get; }
        public IReadOnlyList<TreeNodeViewModel> Children => children.Value;
        public IReadOnlyList<NodeDetailViewModel> Details => details.Value;
        public IReadOnlyList<KeyValueViewModel> Table => table.Value;

        public IEnumerable<KeyValueViewModel> CreateTable()
        {
            return new[]
            {
                new KeyValueViewModel("Name", Model.Name),
                new KeyValueViewModel("Node path", Model.Path),
                new KeyValueViewModel("Container path", Model.Container.Path),
                new KeyValueViewModel("Container address", Model.Container.Address.ToString())
            }.Concat(Format.Table);
        }
    }
}
