using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace DigiMixer.Wpf;
/// <summary>
/// Interaction logic for MixerWindow.xaml
/// </summary>
public partial class MixerWindow : Window
{
    private bool closed;

    public MixerWindow()
    {
        InitializeComponent();
    }

    internal async Task StartConnecting(ILogger logger, Func<IMixerApi> apiFactory)
    {
        DataContext = new ConnectingMixerViewModel();
        while (!closed)
        {
            try
            {
                var mixer = await Mixer.Create(logger, apiFactory);
                DataContext = new MixerViewModel(mixer);
                return;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error creating mixer... retrying in a few seconds.");
                await Task.Delay(3000);
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        closed = true;
        base.OnClosed(e);
        if (DataContext is IDisposable disp)
        {
            disp.Dispose();
        }
    }
}
