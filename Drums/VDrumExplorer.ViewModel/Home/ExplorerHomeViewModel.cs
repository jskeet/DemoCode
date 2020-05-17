// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Windows.Input;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Audio;
using VDrumExplorer.Proto;
using VDrumExplorer.ViewModel.Audio;
using VDrumExplorer.ViewModel.Data;
using VDrumExplorer.ViewModel.Dialogs;

namespace VDrumExplorer.ViewModel.Home
{
    public class ExplorerHomeViewModel : ViewModelBase
    {
        private readonly IViewServices viewServices;

        public SharedViewModel SharedViewModel { get; }

        public ICommand LoadKitFromDeviceCommand { get; }
        public ICommand LoadModuleFromDeviceCommand { get; }
        public ICommand RecordInstrumentAudioCommand { get; }
        public ICommand SaveLogCommand { get; }
        public ICommand LoadFileCommand { get; }

        public LogViewModel Log => SharedViewModel.LogViewModel;

        public ExplorerHomeViewModel(IViewServices viewServices, SharedViewModel shared)
        {
            this.viewServices = viewServices;
            SharedViewModel = shared;
            LoadModuleFromDeviceCommand = new DelegateCommand(LoadModuleFromDevice, true);
            LoadKitFromDeviceCommand = new DelegateCommand(LoadKitFromDevice, true);
            RecordInstrumentAudioCommand = new DelegateCommand(RecordInstrumentAudio, true);
            SaveLogCommand = new DelegateCommand(SaveLog, true);
            LoadFileCommand = new DelegateCommand(LoadFile, true);
        }

        private int loadKitFromDeviceNumber = 1;
        public int LoadKitFromDeviceNumber
        {
            get => loadKitFromDeviceNumber;
            set => SetProperty(ref loadKitFromDeviceNumber, SharedViewModel.ConnectedDeviceSchema.ValidateKitNumber(value));
        }

        private void LoadModuleFromDevice()
        {
        }

        private void LoadKitFromDevice()
        {
        }

        private void RecordInstrumentAudio()
        {
            var vm = new InstrumentAudioRecorderViewModel(SharedViewModel);
            viewServices.ShowInstrumentRecorderDialog(vm);
            // TODO: On success, open the InstrumentAudioExplorer?
        }

        private void SaveLog()
        {
            var file = viewServices.ShowSaveFileDialog(FileFilters.TextFiles);
            if (file == null)
            {
                return;
            }
            SharedViewModel.LogViewModel.Save(file);
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
                SharedViewModel.Log($"Error loading {file}", ex);
                return;
            }
            // TODO: Potentially declare an IDrumData interface with the Schema property and Validate method.
            switch (loaded)
            {
                case Kit kit:
                    {
                        var vm = new KitExplorerViewModel(viewServices, SharedViewModel, kit) { FileName = file };
                        viewServices.ShowKitExplorer(vm);
                        break;
                    }
                case Module module:
                    {
                        var vm = new ModuleExplorerViewModel(viewServices, SharedViewModel, module) { FileName = file };
                        viewServices.ShowModuleExplorer(vm);
                        break;
                    }
                case ModuleAudio audio:
                    {
                        // TODO: Maybe refactor for consistency?
                        var vm = new InstrumentAudioExplorerViewModel(viewServices, SharedViewModel, audio, file);
                        viewServices.ShowInstrumentAudioExplorer(vm);
                        break;
                    }
                default:
                    SharedViewModel.Log($"Unknown file data type");
                    break;
            }
        }

    }
}
