using DigiMixer.Wpf.Utilities;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;

namespace DigiMixer.Wpf;

public partial class App : Application
{
    internal MemoryLoggerProvider Log { get; private set; }

    public static new App Current => (App) Application.Current;

    public App()
    {
        Log = new MemoryLoggerProvider();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        var logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigiMixer", "Logs");
        Log.SaveToDirectory(logsDirectory, "log");
    }

    internal class MemoryLogger : ILogger
    {
        private List<string> logEntries = new List<string>();

        public IDisposable BeginScope<TState>(TState state) => Task.CompletedTask;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            lock (logEntries)
            {
                logEntries.Add(formatter(state, exception));
            }
        }

        public void Save(string path)
        {
            lock (logEntries)
            {
                File.WriteAllLines(path, logEntries);
            }
        }
    }
}
