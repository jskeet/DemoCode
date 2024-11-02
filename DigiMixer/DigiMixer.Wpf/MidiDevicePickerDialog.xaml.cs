using System.Windows;
using System.Windows.Input;

namespace DigiMixer.Wpf;
/// <summary>
/// Interaction logic for MidiDevicePickerDialog.xaml
/// </summary>
public partial class MidiDevicePickerDialog : Window
{
    public MidiDevicePickerDialog()
    {
        InitializeComponent();
    }

    private void Cancel(object sender, RoutedEventArgs e) => DialogResult = false;
    private void AcceptSelection(object sender, MouseButtonEventArgs e) => DialogResult = true;
}
