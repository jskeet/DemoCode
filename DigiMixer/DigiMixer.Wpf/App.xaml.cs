using JonSkeet.WpfLogging;
using JonSkeet.WpfUtil;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.Windows;

namespace DigiMixer.Wpf;

public partial class App : Application
{
    public static new App Current => (App) Application.Current;
    public MemoryLoggerProvider Log { get; private set; }
    private MainWindowViewModel viewModel;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        FileLocations.MaybeCreateAppDataDirectory();
        FileLocations.MaybeCreateInitialConfig();

        var config = JsonUtilities.LoadJson<DigiMixerAppConfig>(FileLocations.ConfigFile);
        Log = new MemoryLoggerProvider(SystemClock.Instance, config.Logging);
        Log.CreateLogger("App").LogInformation("DigiMixer version: {version}", Versions.AppVersion);

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Log.CreateLogger("AppDomain").LogCritical(args.ExceptionObject as Exception,
                "Unhandled exception in AppDomain; check for async void methods");
            if (args.IsTerminating)
            {
                SaveLog();
            }
        };

        Dispatcher.UnhandledException += (sender, args) =>
        {
            Log.CreateLogger("Dispatcher").LogError(args.Exception, "Error in dispatcher thread");
            args.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            args.SetObserved();
            Log.CreateLogger("TaskScheduler").LogError(args.Exception, "Error in task scheduler");
        };

        viewModel = new MainWindowViewModel(config, Log.CreateLogger("DigiMixer"));
        await viewModel.InitializeViewModels();
        ShowMainWindow();
    }

    public void ShowMainWindow()
    {
        if (Thread.CurrentThread != Dispatcher.Thread)
        {
            Dispatcher.Invoke(ShowMainWindow);
            return;
        }
        if (MainWindow is null)
        {
            MainWindow = new MainWindow { DataContext = viewModel };
        }
        MainWindow.Show();
        if (MainWindow.WindowState == WindowState.Minimized)
        {
            MainWindow.WindowState = WindowState.Normal;
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        SaveLog();
        // TODO: We probably want to genuinely block here, which is tricky.
        await (viewModel?.DisposeAsync() ?? ValueTask.CompletedTask);
        base.OnExit(e);
    }

    private void SaveLog() => Log.SaveToDirectory(FileLocations.LoggingDirectory, "log");
}
