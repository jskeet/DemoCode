// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.IO;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.Data
{
    public abstract class DataExplorerViewModel : ViewModelBase<ModuleData>
    {
        private readonly ModuleData data;

        public DelegateCommand EditCommand { get; }
        public DelegateCommand CommitCommand { get; }
        public DelegateCommand CancelEditCommand { get; }

        private string? fileName;
        public string? FileName
        {
            get => fileName;
            set
            {
                if (SetProperty(ref fileName, value))
                {
                    RaisePropertyChanged(nameof(Title));
                }
            }
        }

        protected abstract string ExplorerName { get; }
        public abstract string SaveFileFilter { get; }
        protected abstract void SaveToStream(Stream stream);

        public string Title => FileName is null
            ? $"{ExplorerName} ({Model.Schema.Identifier.Name})"
            : $"{ExplorerName} ({Model.Schema.Identifier.Name}) - {fileName}";

        private ModuleDataSnapshot? snapshot;

        public DataExplorerViewModel(ModuleData data) : base(data)
        {
            this.data = data;
            readOnly = true;
            Root = SingleItemCollection.Of(new DataTreeNodeViewModel(data.LogicalRoot, this));
            SelectedNode = Root[0];
            EditCommand = new DelegateCommand(EnterEditMode, readOnly);
            CommitCommand = new DelegateCommand(CommitEdit, !readOnly);
            CancelEditCommand = new DelegateCommand(CancelEdit, !readOnly);
        }

        private bool readOnly;
        public bool ReadOnly
        {
            get => readOnly;
            private set
            {
                if (SetProperty(ref readOnly, value))
                {
                    EditCommand.Enabled = value;
                    CommitCommand.Enabled = !value;
                    CancelEditCommand.Enabled = !value;
                }
            }
        }

        public void Save()
        {
            using (var stream = File.OpenWrite(FileName))
            {
                SaveToStream(stream);
            }
        }

        private void EnterEditMode()
        {
            snapshot = data.CreateSnapshot();
            ReadOnly = false;
        }

        private void CancelEdit()
        {
            data.LoadSnapshot(snapshot!);
            ReadOnly = true;
        }

        private void CommitEdit()
        {
            ReadOnly = true;
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
