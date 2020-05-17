// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.
using System.ComponentModel;
using System.Windows;
using VDrumExplorer.ViewModel.Dialogs;

namespace VDrumExplorer.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for InstrumentAudioRecorderDialog.xaml
    /// </summary>
    public partial class InstrumentAudioRecorderDialog : Window
    {
        private InstrumentAudioRecorderViewModel ViewModel => (InstrumentAudioRecorderViewModel) DataContext;

        public InstrumentAudioRecorderDialog()
        {
            InitializeComponent();
        }

        private void Cancel(object sender, CancelEventArgs e) =>
            ViewModel.Cancel();
    }
}
