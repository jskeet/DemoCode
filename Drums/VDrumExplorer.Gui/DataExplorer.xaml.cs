// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using VDrumExplorer.Gui.Dialogs;
using VDrumExplorer.Model;
using VDrumExplorer.Proto;
using VDrumExplorer.ViewModel.Data;
using VDrumExplorer.ViewModel.Dialogs;

namespace VDrumExplorer.Gui
{
    /// <summary>
    /// Interaction logic for ModuleExplorer.xaml
    /// </summary>
    public partial class DataExplorer : Window
    {
        private DataExplorerViewModel ViewModel => (DataExplorerViewModel) DataContext;

        public DataExplorer()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) =>
            ViewModel.SelectedNode = (DataTreeNodeViewModel) treeView.SelectedItem;

        // TODO: This doesn't really feel ideal, but my research into handling ApplicationCommands in the ViewModel
        // hasn't shown anything nicer. In particular, the ViewModel would somehow have to request the Save File
        // dialog box...
        private void SaveFileAs(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFile(null);
        }

        private void SaveFile(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFile(ViewModel.FileName);
        }

        private void SaveFile(string? defaultFileName)
        {
            string? newFileName = defaultFileName;
            if (newFileName is null)
            {
                var dialog = new SaveFileDialog { Filter = ViewModel.SaveFileFilter };
                var result = dialog.ShowDialog();
                if (result != true)
                {
                    return;
                }
                newFileName = dialog.FileName;
            }
            ViewModel.FileName = newFileName;
            ViewModel.Save();
        }

        private void OpenCopyInKitExplorer(object sender, ExecutedRoutedEventArgs e)
        {
            var kitNode = (DataTreeNodeViewModel) e.Parameter;
            var module = ((ModuleExplorerViewModel) DataContext).Module;
            var kit = module.ExportKit(kitNode.KitNumber!.Value);
            var kitExplorer = new DataExplorer { DataContext = new KitExplorerViewModel(kit) };
            kitExplorer.Show();
        }

        private void ImportKitFromFile(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Multiselect = false, Filter = "Kit files|*.vkit" };
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
                //Logger.Log($"Error loading {dialog.FileName}", ex);
                return;
            }
            if (!(loaded is Kit kit))
            {
                //Logger.Log("Loaded file was not a kit");
                return;
            }

            var module = ((ModuleExplorerViewModel) DataContext).Module;
            if (!kit.Schema.Identifier.Equals(module.Schema.Identifier))
            {
                //Logger.Log($"Kit was from {kit.Schema.Identifier.Name}; this module is {Schema.Identifier.Name}");
                return;
            }
            var kitNode = (DataTreeNodeViewModel) e.Parameter;
            module.ImportKit(kit, kitNode.KitNumber!.Value);
        }

        private void ExportKit(object sender, ExecutedRoutedEventArgs e)
        {
            var kitNode = (DataTreeNodeViewModel) e.Parameter;
            var module = ((ModuleExplorerViewModel) DataContext).Module;
            var kit = module.ExportKit(kitNode.KitNumber!.Value);

            var dialog = new SaveFileDialog { Filter = "Kit files|*.vkit" };
            var result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            using (var stream = File.Create(dialog.FileName))
            {
                kit.Save(stream);
            }
        }

        private void CopyKit(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: Move everything that creates the CopyKitViewModel into DataTreeNodeViewModel?

            var kitNode = (DataTreeNodeViewModel) e.Parameter;
            var module = ((ModuleExplorerViewModel) DataContext).Module;
            var kit = module.ExportKit(kitNode.KitNumber!.Value);
            var viewModel = new CopyKitViewModel(module, kit);
            var dialog = new CopyKitTarget { DataContext = viewModel };
            var result = dialog.ShowDialog();
            if (result == true)
            {
                module.ImportKit(kit, viewModel.DestinationKitNumber);
            }
        }
    }
}
