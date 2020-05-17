// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.Data
{
    public abstract class DataExplorerViewModel : ViewModelBase<ModuleData>
    {
        protected IViewServices ViewServices { get; }
        private readonly ModuleData data;

        public SharedViewModel SharedViewModel { get; }
        public DelegateCommand EditCommand { get; }
        public DelegateCommand CommitCommand { get; }
        public DelegateCommand CancelEditCommand { get; }
        public DelegateCommand PlayNoteCommand { get; }
        // There are app commands of course, but it's not clear how we bind them.
        public DelegateCommand SaveFileCommand { get; }
        public DelegateCommand SaveFileAsCommand { get; }
        public CommandBase CopyDataToDeviceCommand { get; }

        public void SaveHandler(object sender, EventArgs e)
        {
        }

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

        public abstract ICommand OpenCopyInKitExplorerCommand { get; }
        public abstract ICommand CopyKitCommand { get; }
        public abstract ICommand ImportKitFromFileCommand { get; }
        public abstract ICommand ExportKitCommand { get; }

        protected abstract string ExplorerName { get; }
        public abstract string SaveFileFilter { get; }
        protected abstract void SaveToStream(Stream stream);
        // Ugly, but simple.
        public bool IsKitExplorer => this is KitExplorerViewModel;
        public bool IsModuleExplorer => !IsKitExplorer;

        public string CopyDataTitle => IsKitExplorer ? "Copy Kit" : "Copy Data";

        public string Title => FileName is null
            ? $"{ExplorerName} ({Model.Schema.Identifier.Name})"
            : $"{ExplorerName} ({Model.Schema.Identifier.Name}) - {fileName}";

        public IReadOnlyList<int> MidiChannels { get; } = Enumerable.Range(1, 16).ToList();

        private int selectedMidiChannel = 10;
        public int SelectedMidiChannel
        {
            get => selectedMidiChannel;
            // No validation, as we assume this is in a drop-down for now.
            set => SetProperty(ref selectedMidiChannel, value);
        }

        public int MinAttack => 1;
        public int MaxAttack => 127;

        private int attack = 80;
        public int Attack
        {
            get => attack;
            set => SetProperty(ref attack, value);
        }

        private ModuleDataSnapshot? snapshot;

        public DataExplorerViewModel(IViewServices viewServices, SharedViewModel shared, ModuleData data) : base(data)
        {
            this.ViewServices = viewServices;
            SharedViewModel = shared;
            this.data = data;
            readOnly = true;
            EditCommand = new DelegateCommand(EnterEditMode, readOnly);
            CommitCommand = new DelegateCommand(CommitEdit, !readOnly);
            CancelEditCommand = new DelegateCommand(CancelEdit, !readOnly);
            PlayNoteCommand = new DelegateCommand(PlayNote, shared.DeviceConnected);
            SaveFileCommand = new DelegateCommand(SaveFile, true);
            SaveFileAsCommand = new DelegateCommand(SaveFileAs, true);
            CopyDataToDeviceCommand = new DelegateCommand(CopyDataToDevice, true);
            Root = SingleItemCollection.Of(new DataTreeNodeViewModel(data.LogicalRoot, this));
            SelectedNode = Root[0];
        }

        public void Log(string text) => SharedViewModel.Log(text);
        public void Log(string text, Exception exception) => SharedViewModel.Log(text, exception);

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

        private void SaveFileAs() => SaveFileImpl(null);

        private void SaveFile() => SaveFileImpl(FileName);

        private void SaveFileImpl(string? defaultFileName)
        {
            string? fileName = defaultFileName;
            if (fileName is null)
            {
                fileName = ViewServices.ShowSaveFileDialog(SaveFileFilter);
                if (fileName is null)
                {
                    return;
                }
                FileName = fileName;
            }
            using (var stream = File.OpenWrite(fileName))
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
            set
            {
                if (SetProperty(ref selectedNode, value))
                {
                    PlayNoteCommand.Enabled = SelectedNode?.MidiNotePath is object;
                    SelectedNodeDetails = selectedNode?.CreateDetails();
                }
            }
        }

        private IReadOnlyList<IDataNodeDetailViewModel>? selectedNodeDetails;
        public IReadOnlyList<IDataNodeDetailViewModel>? SelectedNodeDetails
        {
            get => selectedNodeDetails;
            set => SetProperty(ref selectedNodeDetails, value);
        }

        private void PlayNote()
        {
            var midiClient = SharedViewModel.ConnectedDevice;
            if (midiClient is null)
            {
                return;
            }
            var midiNote = SelectedNode?.GetMidiNote();
            if (midiNote is null)
            {
                return;
            }
            
            midiClient.PlayNote(SelectedMidiChannel, midiNote.Value, Attack);
        }

        protected abstract void CopyDataToDevice();
    }
}
