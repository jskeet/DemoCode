using DigiMixer.Core;
using DigiMixer.Mackie;
using DigiMixer.Osc;
using DigiMixer.QuSeries;
using DigiMixer.UCNet;
using DigiMixer.UiHttp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

    private async void LaunchUi24R(object sender, RoutedEventArgs e) =>        
        await Launch(() => new UiHttpMixerApi(CreateLogger("Ui24r"), "192.168.1.57"));

    private async void LaunchXR18(object sender, RoutedEventArgs e) =>
        await Launch(() => XAir.CreateMixerApi(CreateLogger("XR18"), "192.168.1.41"));

    private async void LaunchX32(object sender, RoutedEventArgs e) =>
        await Launch(() => X32.CreateMixerApi(CreateLogger("X32"), "192.168.1.62"));

    private async void LaunchXR16(object sender, RoutedEventArgs e) =>
        await Launch(() => XAir.CreateMixerApi(CreateLogger("XR16"), "192.168.1.185"));

    private async void LaunchM18(object sender, RoutedEventArgs e) =>
        await Launch(() => Rcf.CreateMixerApi(CreateLogger("M18"), "192.168.1.58"));

    private async void LaunchDL16S(object sender, RoutedEventArgs e) =>
        await Launch(() => new MackieMixerApi(CreateLogger("DL16S"), "192.168.1.59"));

    private async void Launch16R(object sender, RoutedEventArgs e) =>
        await Launch(() => StudioLive.CreateMixerApi(CreateLogger("16R"), "192.168.1.61"));

    private async void LaunchQuSB(object sender, RoutedEventArgs e) =>
        await Launch(() => QuMixer.CreateMixerApi(CreateLogger("Qu-SB"), "192.168.1.60"));

    private async Task Launch(Func<IMixerApi> apiFactory)
    {
        var mixer = await Mixer.Create(CreateLogger("Mixer"), apiFactory);
        var vm = new MixerViewModel(mixer);
        var window = new MixerWindow { DataContext = vm };
        window.Show();
    }

    private static ILogger CreateLogger(string name) => App.Current.Log.CreateLogger(name);
}
