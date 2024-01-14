using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
