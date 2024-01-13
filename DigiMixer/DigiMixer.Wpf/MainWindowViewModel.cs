using DigiMixer.Controls;
using JonSkeet.WpfLogging;
using JonSkeet.WpfUtil;
using Microsoft.Extensions.Logging;
using System.Windows.Input;

namespace DigiMixer.Wpf;

public class MainWindowViewModel : ViewModelBase, IAsyncDisposable
{
    private readonly ILogger logger;
    public string Title => $"DigiMixer ({Versions.AppVersion})";

    private DigiMixerAppConfig config;

    private DigiMixerViewModel mixer;
    public DigiMixerViewModel Mixer
    {
        get => mixer;
        private set => SetProperty(ref mixer, value);
    }

    private PeripheralController peripheralController;
    public PeripheralController PeripheralController
    {
        get => peripheralController;
        private set => SetProperty(ref peripheralController, value);
    }

    public ICommand ShowLogCommand { get; }
    public ICommand ShowCreditsCommand { get; }
    public ICommand ReconfigureCommand { get; }

    public MainWindowViewModel(DigiMixerAppConfig config, ILogger logger)
    {
        this.config = config;
        this.logger = logger;
        ShowLogCommand = ActionCommand.FromAction(ShowLog);
        ShowCreditsCommand = ActionCommand.FromAction(() => new CreditsWindow().ShowDialog());
        ReconfigureCommand = ActionCommand.FromAction(Reconfigure);
        InitializeViewModels();
    }

    private void InitializeViewModels()
    {
        var log = App.Current.Log;
        var mixerVm = new DigiMixerViewModel(log.CreateLogger("Mixer"), config.Mixer);
        var peripheralController = PeripheralController.Create(log, mixerVm, config.EnablePeripherals);
        // Start the peripheral monitoring task, but just log if it fails.
        peripheralController.Start().Ignore(log.CreateLogger("PeripheralControllerMonitoring"));
        Mixer = mixerVm;
        PeripheralController = peripheralController;
    }

    public async ValueTask DisposeAsync()
    {
        Mixer.DisposeWithCatch(logger);
        await PeripheralController.DisposeAsyncWithCatch(logger);
    }

    private void ShowLog() => new LogWindow { DataContext = new LogViewModel(App.Current.Log) }.Show();

    private async void Reconfigure()
    {
        // TODO: prompt for the new configuration.

        // Dispose of the old mixer, then wait for a second to allow resource to actually get freed
        // and become accessible again.
        await DisposeAsync();
        await Task.Delay(1000);

        // Now create the new view models, with a specific error message if we fail to recreate them.
        try
        {
            InitializeViewModels();
        }
        catch (Exception ex)
        {
            // TODO: Save the configuration in some backup location?
            Dialogs.ShowErrorDialog("Error when reconfiguring",
                "An error occurred when trying to use the new configuration.\r\nPlease restart DigiMixer and try again.\r\nThe new configuration has not been saved.");
            logger.LogError(ex, "Error while reconfiguring");
            return;
        }

        // Only save the file when we've successfully at least created the view model.
        JsonUtilities.SaveJson(FileLocations.ConfigFile, config);
    }
}
