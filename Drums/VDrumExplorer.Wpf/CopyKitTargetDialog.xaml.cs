// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Windows;
using System.Windows.Controls;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Layout;

namespace VDrumExplorer.Wpf
{
    /// <summary>
    /// Interaction logic for SelectKitDialog.xaml
    /// </summary>
    public partial class CopyKitTargetDialog : Window
    {
        private readonly ModuleData data;
        private readonly ModuleSchema schema;

        public VisualTreeNode SelectedKit { get; set; }

        public CopyKitTargetDialog()
        {
            InitializeComponent();
        }

        public CopyKitTargetDialog(ModuleSchema schema, ModuleData data, VisualTreeNode sourceKitNode) : this()
        {
            this.schema = schema;
            this.data = data;
            sourceKitName.Content = sourceKitNode.KitOnlyDescription.Format(sourceKitNode.Context, data);
            // This is done after other control initialization so that everything is set up.
            // It would probably be more elegant to do everything in a view model and use binding,
            // admittedly.
            kitNumber.TextChanged += HandleKitNumberChanged;
            HandleKitNumberChanged(null, null);
            kitNumber.PreviewTextInput += TextConversions.CheckDigits;
        }

        private void HandleKitNumberChanged(object sender, TextChangedEventArgs e)
        {
            TextConversions.TryGetKitRoot(kitNumber.Text, schema, null, out var root);
            SelectedKit = root;
            acceptButton.IsEnabled = root != null;
            kitName.Content = root?.KitOnlyDescription.Format(root.Context, data);
        }

        private void Accept(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        
        private void Cancel(object sender, RoutedEventArgs e)
        {
            SelectedKit = null;
            DialogResult = false;
            Close();
        }
    }
}
