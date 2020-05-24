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
        // Note: exposing the root like this is somewhat ugly, but it means we can have "4 command instances which take a parameter"
        // rather than "4 command instances per tree view node". It's relatively tricky to get back to the root datacontext
        // within the XAML for a menu item.
        public DataExplorerViewModel Root { get; }

        public new DataTreeNode Model => base.Model;

        private readonly Lazy<IReadOnlyList<DataTreeNodeViewModel>> children;

        public DataFieldFormattableString Format { get; }
        public IReadOnlyList<DataTreeNodeViewModel> Children => children.Value;

        public int? KitNumber => Model.SchemaNode.KitNumber;
        public bool KitContextCommandsEnabled => Root.IsModuleExplorer && KitNumber.HasValue;

        public string? MidiNotePath => Model.SchemaNode.MidiNotePath;

        public int? GetMidiNote() => Model.GetMidiNote();

        public DataTreeNodeViewModel(DataTreeNode model, DataExplorerViewModel root) : base(model)
        {
            Root = root;
            Format = model.Format;
            children = Lazy.Create(() => model.Children.ToReadOnlyList(child => new DataTreeNodeViewModel(child, root)));
        }

        internal IReadOnlyList<IDataNodeDetailViewModel> CreateDetails() => Model.Details.ToReadOnlyList(CreateDetail);

        private IDataNodeDetailViewModel CreateDetail(IDataNodeDetail detail) =>
            detail switch
            {
                ListDataNodeDetail model => new ListDataNodeDetailViewModel(model),
                FieldContainerDataNodeDetail model => new FieldContainerDataNodeDetailViewModel(model, Root),
                _ => throw new ArgumentException($"Unknown detail type: {detail?.GetType()}")
            };
    }
}
