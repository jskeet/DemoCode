using DigiMixer.Core;
using DigiMixer.CqSeries;
using DigiMixer.Mackie;
using DigiMixer.Osc;
using DigiMixer.QuSeries;
using DigiMixer.UCNet;
using DigiMixer.UiHttp;
using Microsoft.Extensions.Logging;
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

    private void LaunchUi24R(object sender, RoutedEventArgs e) =>
        Launch(() => new UiHttpMixerApi(CreateLogger("Ui24r"), "192.168.1.57"));

    private void LaunchXR18(object sender, RoutedEventArgs e) =>
        Launch(() => XAir.CreateMixerApi(CreateLogger("XR18"), "192.168.1.41"));

    private void LaunchX32(object sender, RoutedEventArgs e) =>
        Launch(() => X32.CreateMixerApi(CreateLogger("X32"), "192.168.1.62"));

    private void LaunchXR16(object sender, RoutedEventArgs e) =>
        Launch(() => XAir.CreateMixerApi(CreateLogger("XR16"), "192.168.1.185"));

    private void LaunchM18(object sender, RoutedEventArgs e) =>
        Launch(() => Rcf.CreateMixerApi(CreateLogger("M18"), "192.168.1.58"));

    private void LaunchDL16S(object sender, RoutedEventArgs e) =>
        Launch(() => new MackieMixerApi(CreateLogger("DL16S"), "192.168.1.59"));

    private void LaunchDL32R(object sender, RoutedEventArgs e) =>
        Launch(() => new MackieMixerApi(CreateLogger("DL32R"), "192.168.1.68"));

    private void Launch16R(object sender, RoutedEventArgs e) =>
        Launch(() => StudioLive.CreateMixerApi(CreateLogger("16R"), "192.168.1.61"));

    private void LaunchQuSB(object sender, RoutedEventArgs e) =>
        Launch(() => QuMixer.CreateMixerApi(CreateLogger("Qu-SB"), "192.168.1.60"));

    private void LaunchCQ20B(object sender, RoutedEventArgs e) =>
        Launch(() => CqMixer.CreateMixerApi(CreateLogger("CQ20B"), "192.168.1.85"));

    private void Launch(Func<IMixerApi> apiFactory)
    {
        var window = new MixerWindow();
        window.StartConnecting(CreateLogger("Mixer"), apiFactory).ContinueWith(task => { });
        window.Show();
    }

    private static ILogger CreateLogger(string name) => App.Current.Log.CreateLogger(name);
}
