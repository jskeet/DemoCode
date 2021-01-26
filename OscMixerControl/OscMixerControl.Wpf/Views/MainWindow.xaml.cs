// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscMixerControl.Wpf.ViewModels;
using System.Windows;

namespace OscMixerControl.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private async void Connect(object sender, RoutedEventArgs e) =>
            await ViewModel.ConnectAsync();

        private async void SendCommand(object sender, RoutedEventArgs e) =>
            await ViewModel.SendCommandAsync();
    }
}
