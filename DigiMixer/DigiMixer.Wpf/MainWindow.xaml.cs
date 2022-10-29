using DigiMixer.Osc;
using DigiMixer.UiHttp;
using Microsoft.Extensions.Logging;
using OscMixerControl;
using System.Windows;

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
        var api = new UiHttpMixerApi(CreateLogger("Ui24r"), "192.168.1.57");        
        var mixer = await Mixer.Detect(api);
        Launch(mixer);
    }

    private async void LaunchXR18(object sender, RoutedEventArgs e)
    {
        var api = XAir.CreateMixerApi(CreateLogger("XR18"), "192.168.1.41");
        var mixer = await Mixer.Detect(api);
        Launch(mixer);
    }

    private async void LaunchXR16(object sender, RoutedEventArgs e)
    {
        var api = XAir.CreateMixerApi(CreateLogger("XR16"), "192.168.1.185");
        var mixer = await Mixer.Detect(api);
        Launch(mixer);
    }

    private async void LaunchM18(object sender, RoutedEventArgs e)
    {
        var api = Rcf.CreateMixerApi(CreateLogger("M18"), "192.168.1.58");
        var mixer = await Mixer.Detect(api);
        Launch(mixer);
    }

    private void Launch(Mixer mixer)
    {
        var vm = new MixerViewModel(mixer);        
        var window = new MixerWindow { DataContext = vm };
        window.Show();
    }

    private static ILogger CreateLogger(string name) => App.Current.Log.CreateLogger(name);
}
