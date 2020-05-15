// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Windows;
using VDrumExplorer.ViewModel.Audio;

namespace VDrumExplorer.Gui
{
    /// <summary>
    /// Interaction logic for InstrumentAudioExplorer.xaml
    /// </summary>
    public partial class InstrumentAudioExplorer : Window
    {
        private InstrumentAudioExplorerViewModel ViewModel => (InstrumentAudioExplorerViewModel) DataContext;

        public InstrumentAudioExplorer()
        {
            InitializeComponent();
        }

        /*
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) =>
            ViewModel.SelectedGroup = (InstrumentGroupAudioViewModel) treeView.SelectedItem;
            */
        private async void PlayAudio(object sender, RoutedEventArgs e)
        {
            await ViewModel.PlayAudio();
        }
    }
}
