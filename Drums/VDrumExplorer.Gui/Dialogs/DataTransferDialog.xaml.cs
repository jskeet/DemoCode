// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.ComponentModel;
using System.Windows;
using VDrumExplorer.ViewModel.Dialogs;

namespace VDrumExplorer.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for DataTransferDialog.xaml
    /// </summary>
    public partial class DataTransferDialog : Window
    {
        public DataTransferDialog()
        {
            InitializeComponent();
        }

        private void HandleClosing(object sender, CancelEventArgs e) =>
            ((DataTransferViewModel) DataContext).CancelCommand.Execute(null);
    }
}
