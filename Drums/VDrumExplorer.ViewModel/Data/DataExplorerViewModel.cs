// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Data;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.Data
{
    public class DataExplorerViewModel : ViewModelBase<ModuleData>
    {
        public string Title => "TBD";

        public DataExplorerViewModel(ModuleData data) : base(data)
        {
            Root = SingleItemCollection.Of(new DataTreeNodeViewModel(data.LogicalRoot));
            SelectedNode = Root[0];
        }

        public SingleItemCollection<DataTreeNodeViewModel> Root { get; }

        private DataTreeNodeViewModel? selectedNode;
        public DataTreeNodeViewModel? SelectedNode
        {
            get => selectedNode;
            set => SetProperty(ref selectedNode, value);
        }

    }
}
