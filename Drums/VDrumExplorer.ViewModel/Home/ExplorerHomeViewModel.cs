// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using VDrumExplorer.Model.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Audio;
using VDrumExplorer.Proto;
using VDrumExplorer.Utility;
using VDrumExplorer.ViewModel.Audio;
using VDrumExplorer.ViewModel.Data;
using VDrumExplorer.ViewModel.Dialogs;
using VDrumExplorer.ViewModel.LogicalSchema;

namespace VDrumExplorer.ViewModel.Logging
{
    public class ExplorerHomeViewModel : ViewModelBase
    {
        private readonly IViewServices viewServices;
        private readonly ILogger logger;
        private readonly IAudioDeviceManager audioDeviceManager;

        public LogViewModel LogViewModel { get; }
        public DeviceViewModel DeviceViewModel { get; }

        public ICommand OpenSchemaExplorerCommand { get; }
        public ICommand LoadKitFromDeviceCommand { get; }
        public ICommand LoadModuleFromDeviceCommand { get; }
        public ICommand RecordInstrumentAudioCommand { get; }
        public ICommand SaveLogCommand { get; }
        public ICommand LoadFileCommand { get; }

        public IReadOnlyList<ModuleIdentifier> KnownSchemas { get; }

        private ModuleIdentifier selectedSchema;
        public ModuleIdentifier SelectedSchema
        {
            get => selectedSchema;
            set => SetProperty(ref selectedSchema, value);
        }

        public ExplorerHomeViewModel(IViewServices viewServices, LogViewModel logViewModel, DeviceViewModel deviceViewModel, IAudioDeviceManager audioDeviceManager)
        {
            this.viewServices = viewServices;
            this.audioDeviceManager = audioDeviceManager;
            LogViewModel = logViewModel;
            logger = LogViewModel.Logger;
            DeviceViewModel = deviceViewModel;
            OpenSchemaExplorerCommand = new DelegateCommand(OpenSchemaExplorer, true);
            LoadModuleFromDeviceCommand = new DelegateCommand(LoadModuleFromDevice, true);
            LoadKitFromDeviceCommand = new DelegateCommand(LoadKitFromDevice, true);
            RecordInstrumentAudioCommand = new DelegateCommand(RecordInstrumentAudio, true);
            SaveLogCommand = new DelegateCommand(SaveLog, true);
            LoadFileCommand = new DelegateCommand(LoadFile, true);
            KnownSchemas = ModuleSchema.KnownSchemas.Keys.ToReadOnlyList();
            selectedSchema = KnownSchemas[0];
        }

        private int loadKitFromDeviceNumber = 1;
        public int LoadKitFromDeviceNumber
        {
            get => loadKitFromDeviceNumber;
            set => SetProperty(ref loadKitFromDeviceNumber, (DeviceViewModel.ConnectedDevice?.Schema).ValidateKitNumber(value));
        }

        private void OpenSchemaExplorer() => viewServices.ShowSchemaExplorer(new ModuleSchemaViewModel(ModuleSchema.KnownSchemas[SelectedSchema].Value));

        private async void LoadModuleFromDevice()
        {
            var device = DeviceViewModel.ConnectedDevice;
            if (device is null)
            {
                return;
            }
            var transferViewModel = new DataTransferViewModel<Module>(logger, "Loading module data", "Loading {0}", device.LoadModuleAsync);
            var module = await viewServices.ShowDataTransferDialog(transferViewModel);
            if (module is object)
            {
                var moduleViewModel = new ModuleExplorerViewModel(viewServices, logger, DeviceViewModel, module);
                viewServices.ShowModuleExplorer(moduleViewModel);
            }
        }

        private async void LoadKitFromDevice()
        {
            var device = DeviceViewModel.ConnectedDevice;
            if (device is null)
            {
                return;
            }
            var kitNumber = LoadKitFromDeviceNumber;
            var transferViewModel = new DataTransferViewModel<Kit>(logger, "Loading kit data", "Loading {0}", (progress, token) => device.LoadKitAsync(kitNumber, progress, token));
            var kit = await viewServices.ShowDataTransferDialog(transferViewModel);
            if (kit is object)
            {
                var kitViewModel = new KitExplorerViewModel(viewServices, logger, DeviceViewModel, kit);
                viewServices.ShowKitExplorer(kitViewModel);
            }
        }

        private void RecordInstrumentAudio()
        {
            var vm = new InstrumentAudioRecorderViewModel(viewServices, logger, DeviceViewModel, audioDeviceManager);
            viewServices.ShowInstrumentRecorderDialog(vm);
            // If we finished successfully, show the instrument explorer.
            if (vm.RecordedAudio is ModuleAudio audio)
            {
                var resultVm = new InstrumentAudioExplorerViewModel(audioDeviceManager, audio, vm.Settings.OutputFile);
                viewServices.ShowInstrumentAudioExplorer(resultVm);
            }
        }

        private void SaveLog()
        {
            var file = viewServices.ShowSaveFileDialog(FileFilters.LogFiles);
            if (file == null)
            {
                return;
            }
            LogViewModel.Save(file);
        }

        private void LoadFile()
        {
            var file = viewServices.ShowOpenFileDialog(FileFilters.AllExplorerFiles);
            if (file == null)
            {
                return;
            }
            object loaded;
            try
            {
                loaded = ProtoIo.LoadModel(file);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error loading {file}");
                return;
            }
            // TODO: Potentially declare an IDrumData interface with the Schema property and Validate method.
            switch (loaded)
            {
                case Kit kit:
                    {
                        var vm = new KitExplorerViewModel(viewServices, logger, DeviceViewModel, kit) { FileName = file };
                        viewServices.ShowKitExplorer(vm);
                        break;
                    }
                case Module module:
                    {
                        var vm = new ModuleExplorerViewModel(viewServices, logger, DeviceViewModel, module) { FileName = file };
                        viewServices.ShowModuleExplorer(vm);
                        break;
                    }
                case ModuleAudio audio:
                    {
                        // TODO: Maybe refactor for consistency?
                        var vm = new InstrumentAudioExplorerViewModel(audioDeviceManager, audio, file);
                        viewServices.ShowInstrumentAudioExplorer(vm);
                        break;
                    }
                default:
                    logger.LogError($"Unknown file data type");
                    break;
            }
        }

    }
}
