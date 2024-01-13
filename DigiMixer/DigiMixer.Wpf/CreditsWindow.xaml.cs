using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace DigiMixer.Wpf;

/// <summary>
/// Interaction logic for CreditsWindow.xaml
/// </summary>
public partial class CreditsWindow : Window
{
    public CreditsWindow()
    {
        InitializeComponent();
    }

    private void LaunchUri(object sender, RequestNavigateEventArgs e) =>
        Process.Start("explorer.exe", e.Uri.ToString());
}
