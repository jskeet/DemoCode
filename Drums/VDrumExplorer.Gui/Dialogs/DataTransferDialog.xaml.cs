// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace VDrumExplorer.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for DataTransferDialog.xaml
    /// </summary>
    public partial class DataTransferDialog : Window
    {
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public DataTransferDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The cancel button always just acts like the Close button. We'll cancel
        /// any current recording in HandleClosing.
        /// </summary>
        private void Cancel(object sender, RoutedEventArgs e) => Close();

        private void HandleClosing(object sender, CancelEventArgs e)
        {
            tokenSource.Cancel();
        }
    }
}
