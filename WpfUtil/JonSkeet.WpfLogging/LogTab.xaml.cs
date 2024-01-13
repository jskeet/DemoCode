// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JonSkeet.WpfLogging;

/// <summary>
/// Interaction logic for LogTab.xaml
/// </summary>
public partial class LogTab : UserControl
{
    private LogViewModel ViewModel => DataContext as LogViewModel;

    public LogTab()
    {
        InitializeComponent();
    }

    private void AddManualEntry(object sender, RoutedEventArgs e) =>
        ViewModel.AddManualEntry();

    private void HandleManualEntryKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ViewModel.AddManualEntry();
            e.Handled = true;
        }
    }
}
