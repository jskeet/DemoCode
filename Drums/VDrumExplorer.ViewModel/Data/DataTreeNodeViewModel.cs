// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using VDrumExplorer.Model.Data.Logical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.Data
{
    public class DataTreeNodeViewModel : ViewModelBase<DataTreeNode>
    {
        private readonly DataExplorerViewModel parent;
        private readonly Lazy<IReadOnlyList<DataTreeNodeViewModel>> children;

        public DataFieldFormattableString Format { get; }
        public IReadOnlyList<DataTreeNodeViewModel> Children => children.Value;

        public int? KitNumber => Model.SchemaNode.KitNumber;
        public bool KitContextCommandsEnabled => parent.IsModuleExplorer && KitNumber.HasValue;

        public string? MidiNotePath => Model.SchemaNode.MidiNotePath;

        public int? GetMidiNote() => Model.GetMidiNote();

        public DataTreeNodeViewModel(DataTreeNode model, DataExplorerViewModel parent) : base(model)
        {
            this.parent = parent;
            Format = model.Format;
            children = Lazy.Create(() => model.Children.ToReadOnlyList(child => new DataTreeNodeViewModel(child, parent)));
        }

        internal IReadOnlyList<IDataNodeDetailViewModel> CreateDetails() => Model.Details.ToReadOnlyList(CreateDetail);

        private IDataNodeDetailViewModel CreateDetail(IDataNodeDetail detail) =>
            detail switch
            {
                ListDataNodeDetail model => new ListDataNodeDetailViewModel(model),
                FieldContainerDataNodeDetail model => new FieldContainerDataNodeDetailViewModel(model, parent),
                _ => throw new ArgumentException($"Unknown detail type: {detail?.GetType()}")
            };
    }
}
