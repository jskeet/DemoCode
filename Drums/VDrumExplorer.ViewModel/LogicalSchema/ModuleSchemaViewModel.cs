// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.LogicalSchema
{
    public sealed class ModuleSchemaViewModel : ViewModelBase<ModuleSchema>
    {
        public string Title => $"Schema Explorer: {Model.Identifier.Name} (rev 0x{Model.Identifier.SoftwareRevision:x})";

        public ModuleSchemaViewModel(ModuleSchema model) : base(model)
        {
            Root = SingleItemCollection.Of(new TreeNodeViewModel(model.LogicalRoot));
            SelectedNode = Root[0];
        }

        public SingleItemCollection<TreeNodeViewModel> Root { get; }

        private TreeNodeViewModel? selectedNode;
        public TreeNodeViewModel? SelectedNode
        {
            get => selectedNode;
            set => SetProperty(ref selectedNode, value);
        }
    }
}
