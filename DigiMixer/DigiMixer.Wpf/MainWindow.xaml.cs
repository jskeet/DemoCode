using DigiMixer.Osc;
using DigiMixer.UiHttp;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DigiMixer.Wpf;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void LaunchUi24R(object sender, RoutedEventArgs e)
    {
        var api = new UiHttpMixerApi("192.168.1.57", 80);
        var inputs = new[]
        {
            (new InputChannelId(1), default(InputChannelId?)),
            (new InputChannelId(9), new InputChannelId(10)),
            (new InputChannelId(21), new InputChannelId(22)),
        };
        var outputs = new[]
        {
            (new OutputChannelId(1), new OutputChannelId(2)),
            (UiAddresses.MainOutput, (OutputChannelId?) UiAddresses.MainOutputRightMeter)
        };
        var mixer = new Mixer(api, inputs, outputs);
        await Launch(mixer);
    }

    private async void LaunchXR18(object sender, RoutedEventArgs e)
    {
        var api = OscMixerApi.ForUdp("192.168.1.41", 10024);
        var inputs = new[]
        {
            (new InputChannelId(1), default(InputChannelId?)),
            (new InputChannelId(2), default(InputChannelId?)),
        };
        var outputs = new[]
        {
            (new OutputChannelId(1), new OutputChannelId(2)),
            (new OutputChannelId(3), new OutputChannelId(4)),
            (UiAddresses.MainOutput, (OutputChannelId?) UiAddresses.MainOutputRightMeter)
        };
        var mixer = new Mixer(api, inputs, outputs);
        await Launch(mixer);
    }

    private async Task Launch(Mixer mixer)
    {
        var vm = new MixerViewModel(mixer);
        await mixer.Start();
        
        var window = new MixerWindow { DataContext = vm };
        window.Show();
    }
}
