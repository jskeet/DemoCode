using System.Windows;

namespace DigiMixer.Wpf;
/// <summary>
/// Interaction logic for ConfigurationEditorDialog.xaml
/// </summary>
public partial class ConfigurationEditorDialog : Window
{
    public ConfigurationEditorDialog()
    {
        InitializeComponent();
    }

    private void AcceptConfiguration(object sender, RoutedEventArgs e) => DialogResult = true;
    private void Cancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
