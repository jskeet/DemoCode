// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Windows.Input;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Json;
using VDrumExplorer.Proto;
using VDrumExplorer.ViewModel.Dialogs;

namespace VDrumExplorer.ViewModel.Data
{
    public class ModuleExplorerViewModel : DataExplorerViewModel
    {
        public Module Module { get; }

        public ModuleExplorerViewModel(IViewServices viewServices, ILogger logger, DeviceViewModel deviceViewModel, Module module)
            : base(viewServices, logger, deviceViewModel, module.Data)
        {
            Module = module;
            OpenCopyInKitExplorerCommand = new ConditionallyEnabledDelegateCommand<DataTreeNodeViewModel>(viewServices, OpenCopyInKitExplorer, IsKitNode);
            CopyKitCommand = new ConditionallyEnabledDelegateCommand<DataTreeNodeViewModel>(viewServices, CopyKit, IsKitNode);
            ImportKitFromFileCommand = new ConditionallyEnabledDelegateCommand<DataTreeNodeViewModel>(viewServices, ImportKitFromFile, IsKitNode);
            ExportKitCommand = new ConditionallyEnabledDelegateCommand<DataTreeNodeViewModel>(viewServices, ExportKit, IsKitNode);

            bool IsKitNode(DataTreeNodeViewModel node) => node?.KitNumber is not null;
        }

        protected override string ExplorerName =>  "Module Explorer";
        public override string SaveFileFilter => FileFilters.ModuleFiles;

        protected override void SaveToStream(Stream stream) => Module.Save(stream);

        public override ICommand OpenCopyInKitExplorerCommand { get; }
        public override ICommand CopyKitCommand { get; }
        public override ICommand ImportKitFromFileCommand { get; }
        public override ICommand ExportKitCommand { get; }

        private void OpenCopyInKitExplorer(DataTreeNodeViewModel kitNode)
        {
            if (kitNode.KitNumber is not int kitNumber)
            {
                return;
            }
            var kit = Module.ExportKit(kitNumber);
            var viewModel = new KitExplorerViewModel(ViewServices, Logger, DeviceViewModel, kit);
            ViewServices.ShowKitExplorer(viewModel);
        }

        private void CopyKit(DataTreeNodeViewModel kitNode)
        {
            if (kitNode.KitNumber is not int kitNumber)
            {
                return;
            }
            var kit = Module.ExportKit(kitNumber);
            var viewModel = new CopyKitViewModel(Module, kit);
            var destinationKitNumber = ViewServices.ChooseCopyKitTarget(viewModel);
            if (destinationKitNumber is int destination)
            {
                Module.ImportKit(kit, destination);
            }
        }

        private void ImportKitFromFile(DataTreeNodeViewModel kitNode)
        {
            if (kitNode.KitNumber is not int kitNumber)
            {
                return;
            }
            string? file = ViewServices.ShowOpenFileDialog(FileFilters.KitFiles);
            if (file is null)
            {
                return;
            }
            object loaded;
            try
            {
                loaded = ProtoIo.LoadModel(file, Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading {file}", ex);
                return;
            }
            if (!(loaded is Kit kit))
            {
                Logger.LogError("Loaded file was not a kit");
                return;
            }

            if (!kit.Schema.Identifier.Equals(Module.Schema.Identifier))
            {
                Logger.LogError($"Kit was from {kit.Schema.Identifier.Name}; this module is {Module.Schema.Identifier.Name}");
                return;
            }
            Module.ImportKit(kit, kitNumber);
        }

        private void ExportKit(DataTreeNodeViewModel kitNode)
        {
            if (kitNode.KitNumber is not int kitNumber)
            {
                return;
            }

            var kit = Module.ExportKit(kitNumber);
            var file = ViewServices.ShowSaveFileDialog(FileFilters.KitFiles);
            if (file is null)
            {
                return;
            }
            using (var stream = File.Create(file))
            {
                kit.Save(stream);
            }
        }

        protected override void CopyDataToDevice() => CopyDataToDevice(SelectedNode?.Model, null);

        protected override void ConvertToAlternativeSchema(ModuleSchema schema)
        {
            var converted = new Module(Module.Data.ConvertToSchema(schema, Logger));
            ViewServices.ShowModuleExplorer(new ModuleExplorerViewModel(ViewServices, Logger, DeviceViewModel, converted));
        }

        protected override string FormatAsJson() => Module.ToJson();
    }
}
