// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System;
using System.Windows;
using VDrumExplorer.Gui.Dialogs;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Audio;
using VDrumExplorer.Proto;
using VDrumExplorer.ViewModel.Audio;
using VDrumExplorer.ViewModel.Data;
using VDrumExplorer.ViewModel.Dialogs;
using VDrumExplorer.ViewModel.Home;

namespace VDrumExplorer.Gui
{
    /// <summary>
    /// Interaction logic for ExplorerHome.xaml
    /// </summary>
    public partial class ExplorerHome : Window
    {
        private ExplorerHomeViewModel ViewModel => (ExplorerHomeViewModel) DataContext;

        public ExplorerHome()
        {
            InitializeComponent();
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "All explorer files|*.vdrum;*.vkit;*.vaudio|Module files|*.vdrum|Kit files|*.vkit|Module audio files|*.vaudio"
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            object loaded;
            try
            {
                loaded = ProtoIo.LoadModel(dialog.FileName);
            }
            catch (Exception ex)
            {
                ViewModel.SharedViewModel.Log($"Error loading {dialog.FileName}", ex);
                return;
            }
            // TODO: Potentially declare an IDrumData interface with the Schema property and Validate method.
            switch (loaded)
            {
                case Kit kit:
                    {
                        var vm = new KitExplorerViewModel(ViewModel.SharedViewModel, kit) { FileName = dialog.FileName };
                        new DataExplorer { DataContext = vm }.Show();
                        break;
                    }
                case Module module:
                    {
                        var vm = new ModuleExplorerViewModel(ViewModel.SharedViewModel, module) { FileName = dialog.FileName };
                        new DataExplorer { DataContext = vm }.Show();
                        break;
                    }
                case ModuleAudio audio:
                    {
                        // TODO: Maybe refactor for consistency?
                        var vm = new InstrumentAudioExplorerViewModel(ViewModel.SharedViewModel, audio, dialog.FileName);
                        new InstrumentAudioExplorer { DataContext = vm }.Show();
                        break;
                    }
                default:
                    ViewModel.SharedViewModel.Log($"Unknown file data type");
                    break;
            }
        }

        private void SaveLog(object sender, RoutedEventArgs e)
        {

        }

        private void LoadModuleFromDevice(object sender, RoutedEventArgs e)
        {

        }

        private void LoadKitFromDevice(object sender, RoutedEventArgs e)
        {

        }

        private void RecordInstrumentsFromDevice(object sender, RoutedEventArgs e)
        {
            var vm = new InstrumentAudioRecorderViewModel(ViewModel.SharedViewModel);
            new InstrumentAudioRecorder { DataContext = vm }.ShowDialog();
            // TODO: On success, open the InstrumentAudioExplorer?
        }
    }
}
