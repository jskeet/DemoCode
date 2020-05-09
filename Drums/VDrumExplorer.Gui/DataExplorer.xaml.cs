// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using VDrumExplorer.ViewModel.Data;

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
    }
}
