// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Windows;
using VDrumExplorer.ViewModel.LogicalSchema;

namespace VDrumExplorer.Gui
{
    /// <summary>
    /// Interaction logic for SchemaExplorer.xaml
    /// </summary>
    public partial class SchemaExplorer : Window
    {
        public SchemaExplorer()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ((ModuleSchemaViewModel) DataContext).SelectedNode = (TreeNodeViewModel) treeView.SelectedItem;
        }
    }
}
