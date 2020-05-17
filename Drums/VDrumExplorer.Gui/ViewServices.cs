// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System;
using VDrumExplorer.Gui.Dialogs;
using VDrumExplorer.ViewModel;
using VDrumExplorer.ViewModel.Audio;
using VDrumExplorer.ViewModel.Data;
using VDrumExplorer.ViewModel.Dialogs;

namespace VDrumExplorer.Gui
{
    internal class ViewServices : IViewServices
    {
        internal static ViewServices Instance { get; } = new ViewServices();

        private ViewServices()
        {
        }

        public string? ShowOpenFileDialog(string filter)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = filter
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? ShowSaveFileDialog(string filter)
        {
            SaveFileDialog dialog = new SaveFileDialog { Filter = filter };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public int? ChooseCopyKitTarget(CopyKitViewModel viewModel)
        {
            var dialog = new CopyKitTargetDialog { DataContext = viewModel };
            var result = dialog.ShowDialog();
            return result == true ? viewModel.DestinationKitNumber : default(int?);
        }

        public void ShowKitExplorer(KitExplorerViewModel viewModel) =>
            new DataExplorer { DataContext = viewModel }.Show();

        public void ShowModuleExplorer(ModuleExplorerViewModel viewModel) =>
            new DataExplorer { DataContext = viewModel }.Show();

        public void ShowInstrumentRecorderDialog(InstrumentAudioRecorderViewModel viewModel) =>
            new InstrumentAudioRecorderDialog { DataContext = viewModel }.ShowDialog();

        public void ShowInstrumentAudioExplorer(InstrumentAudioExplorerViewModel viewModel) =>
            new InstrumentAudioExplorer { DataContext = viewModel }.Show();
    }
}
