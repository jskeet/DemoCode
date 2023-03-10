// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Logical;
using VDrumExplorer.Utility;
using VDrumExplorer.ViewModel.Dialogs;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using VDrumExplorer.ViewModel.Home;

namespace VDrumExplorer.ViewModel.Data
{
    public abstract class DataExplorerViewModel : ViewModelBase<ModuleData>
    {
        protected ILogger Logger { get; }
        protected IViewServices ViewServices { get; }
        public DeviceViewModel DeviceViewModel { get; }
        private readonly ModuleData data;
        private readonly bool IsMatchingDeviceConnected;

        public DelegateCommand EditCommand { get; }
        public DelegateCommand CommitCommand { get; }
        public DelegateCommand CancelEditCommand { get; }
        public DelegateCommand PlayNoteCommand { get; }
        public DelegateCommand CopyNodeCommand { get; }
        public DelegateCommand PasteNodeCommand { get; }
        // There are app commands of course, but it's not clear how we bind them.
        public DelegateCommand SaveFileCommand { get; }
        public DelegateCommand SaveFileAsCommand { get; }
        public CommandBase CopyDataToDeviceCommand { get; }
        public virtual ICommand CopyToTemporaryStudioSetCommand => CommandBase.NotImplemented;

        public ICommand ConvertCommand { get; }

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
        public bool IsAerophoneKitExplorer => IsKitExplorer && Model.Schema.Identifier.Equals(ModuleIdentifier.AE10);

        public string CopyDataTitle => IsKitExplorer ? "Copy Kit" : "Copy Data";

        private string SchemaIdentifierDisplayName => $"{Model.Schema.Identifier.Name} rev 0x{Model.Schema.Identifier.SoftwareRevision:x}";

        public string Title => FileName is null
            ? $"{ExplorerName} ({SchemaIdentifierDisplayName})"
            : $"{ExplorerName} ({SchemaIdentifierDisplayName}) - {fileName}";

        public IReadOnlyList<int> MidiChannels { get; } = Enumerable.Range(1, 16).ToList();
        public IReadOnlyList<ModuleIdentifierViewModel> ConvertibleModuleIdentifiers { get; }
        public bool HasConvertibleModuleIdentifiers => ConvertibleModuleIdentifiers.Any();

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

        private NodeSnapshot? copiedSnapshot;
        public NodeSnapshot? CopiedSnapshot
        {
            get => copiedSnapshot;
            set
            {
                if (SetProperty(ref copiedSnapshot, value))
                {
                    PasteNodeCommand.Enabled = IsPasteNodeCommandValid;
                }
            }
        }

        private bool IsPasteNodeCommandValid => copiedSnapshot?.IsValidForTarget(SelectedNode?.Model.SchemaNode) ?? false;

        private ModuleDataSnapshot? snapshot;

        public DataExplorerViewModel(IViewServices viewServices, ILogger logger, DeviceViewModel deviceViewModel, ModuleData data) : base(data)
        {
            Logger = logger;
            this.DeviceViewModel = deviceViewModel;
            this.ViewServices = viewServices;
            this.data = data;
            // TODO: t update this (and the results) if the device is plugged in later.
            IsMatchingDeviceConnected = deviceViewModel?.ConnectedDevice?.Schema.Identifier == data.Schema.Identifier;
            readOnly = true;
            EditCommand = new DelegateCommand(EnterEditMode, readOnly);
            CommitCommand = new DelegateCommand(CommitEdit, !readOnly);
            CancelEditCommand = new DelegateCommand(CancelEdit, !readOnly);
            PlayNoteCommand = new DelegateCommand(PlayNote, IsMatchingDeviceConnected);
            SaveFileCommand = new DelegateCommand(SaveFile, true);
            SaveFileAsCommand = new DelegateCommand(SaveFileAs, true);
            CopyDataToDeviceCommand = new DelegateCommand(CopyDataToDevice, IsMatchingDeviceConnected);
            CopyNodeCommand = new DelegateCommand(CopyNode, true);
            PasteNodeCommand = new DelegateCommand(PasteNode, true);
            Root = SingleItemCollection.Of(new DataTreeNodeViewModel(data.LogicalRoot, this));
            SelectedNode = Root[0];

            ConvertCommand = new DelegateCommand<ModuleIdentifierViewModel>(ConvertToAlternativeSchema, true);
            var moduleId = data.Schema.Identifier;
            ConvertibleModuleIdentifiers = ModuleSchema.KnownSchemas.Keys
                .Where(key => key.Name == moduleId.Name)
                .Except(new[] { moduleId })
                .Select(id => new ModuleIdentifierViewModel(id, true))
                .ToList();
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
            using (var stream = File.Create(fileName))
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
            // We don't really need to log errors here, given that we're going back to where we were.
            data.LoadSnapshot(snapshot!, NullLogger.Instance);
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
                    PlayNoteCommand.Enabled = IsMatchingDeviceConnected && SelectedNode?.MidiNotePath is object;
                    SelectedNodeDetails = selectedNode?.CreateDetails();
                    CopyNodeCommand.Enabled = selectedNode is object;
                    PasteNodeCommand.Enabled = IsPasteNodeCommandValid;
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
            var device = DeviceViewModel.ConnectedDevice;
            if (device is null)
            {
                return;
            }
            var midiNote = SelectedNode?.GetMidiNote();
            if (midiNote is null)
            {
                return;
            }
            
            device.PlayNote(SelectedMidiChannel, midiNote.Value, Attack);
        }

        protected async void CopyDataToDevice(DataTreeNode? node, ModuleAddress? targetAddress)
        {
            var device = DeviceViewModel.ConnectedDevice;
            if (device is null || node is null)
            {
                return;
            }
            // We may or may not really need to bring up the dialog box, but it's simplest to always do that.
            var viewModel = new DataTransferViewModel<string>(Logger, "Copying data to device", "Copying {0}",
                async (progress, token) => { await device.SaveDescendants(node, targetAddress, progress, token); return ""; });
            await ViewServices.ShowDataTransferDialog(viewModel);
        }

        protected abstract void CopyDataToDevice();

        private void CopyNode()
        {
            var sourceNode = SelectedNode?.Model.SchemaNode;
            if (sourceNode is null)
            {
                return;
            }
            var snapshot = Model.CreatePartialSnapshot(sourceNode);
            CopiedSnapshot = new NodeSnapshot(sourceNode, snapshot);
        }

        private void PasteNode()
        {
            var targetNode = SelectedNode?.Model.SchemaNode;
            if (CopiedSnapshot?.IsValidForTarget(targetNode) != true)
            {
                return;
            }
            var relocated = CopiedSnapshot.Data.Relocated(CopiedSnapshot.SourceNode, targetNode!);
            Model.LoadPartialSnapshot(relocated, Logger);
        }

        private void ConvertToAlternativeSchema(ModuleIdentifierViewModel targetId) =>
            ConvertToAlternativeSchema(ModuleSchema.KnownSchemas[targetId.Identifier].Value);

        protected abstract void ConvertToAlternativeSchema(ModuleSchema schema);
    }
}
