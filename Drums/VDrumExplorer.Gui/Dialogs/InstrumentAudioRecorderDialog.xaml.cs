// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;
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
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public InstrumentAudioRecorderDialog()
        {
            InitializeComponent();
        }

        private async void StartRecording(object sender, RoutedEventArgs e)
        {
            await ViewModel.StartRecording(tokenSource.Token);
        }

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            var settings = ViewModel.Settings;
            var dialog = new SaveFileDialog { Filter = settings.OutputFileFilter };
            var result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            settings.OutputFile = dialog.FileName;
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
