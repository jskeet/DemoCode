// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Windows;

namespace VDrumExplorer.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for CopyKitTarget.xaml
    /// </summary>
    public partial class CopyKitTarget : Window
    {
        public CopyKitTarget()
        {
            InitializeComponent();
        }

        private void Copy(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
