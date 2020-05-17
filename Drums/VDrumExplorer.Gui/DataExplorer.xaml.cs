// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Windows;
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
    }
}
