using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using System.Collections.Concurrent;
using System.IO;

namespace DigiMixer.Wpf.Utilities;

internal class MemoryLoggerProvider : ILoggerProvider
{
    private readonly IClock clock;
    private readonly Instant start;

    private readonly ConcurrentDictionary<string, MemoryLogger> loggers =
            new ConcurrentDictionary<string, MemoryLogger>();

    public MemoryLoggerProvider(IClock clock = null)
    {
        this.clock = clock ?? SystemClock.Instance;
        start = this.clock.GetCurrentInstant();
    }

    public ILogger CreateLogger(string categoryName) =>
        loggers.GetOrAdd(categoryName, name => new MemoryLogger(name, clock));

    public void Dispose()
    {
    }

    public void SaveToDirectory(string directory, string filePrefix)
    {
        try
        {
            var timestamp = clock.GetCurrentInstant();
            Directory.CreateDirectory(directory);
            var logFile = Path.Combine(directory, $"{filePrefix}-{timestamp:uuuuMMdd'-'HHmmss}.txt");
            Save(logFile);
        }
        catch
        {
            // Just swallow exceptions at this point.
        }
    }

    public void Save(string file)
    {
        var entries = loggers.ToArray().SelectMany(logger => logger.Value.Snapshot()).OrderBy(entry => entry.Timestamp);
        var formattedStart = InstantPattern.General.Format(start);
        var formattedEnd = InstantPattern.General.Format(clock.GetCurrentInstant());
        string[] prefix =
        {
                $"DigiMixer Log ({formattedStart} to {formattedEnd})",
                "----"
        };
        var lines = prefix.Concat(entries.Select(entry => entry.ToFileFormat()));
        File.WriteAllLines(file, lines);
    }


    private class MemoryLogger : ILogger
    {
        private readonly string categoryName;
        private readonly IClock clock;
        private readonly ConcurrentQueue<LogEntry> entries;

        private readonly IExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

        internal MemoryLogger(string categoryName, IClock clock)
        {
            this.categoryName = categoryName;
            this.clock = clock;
            this.entries = new ConcurrentQueue<LogEntry>();
        }

        public LogEntry[] Snapshot() => entries.ToArray();

        public IDisposable BeginScope<TState>(TState state) => scopeProvider.Push(state);

        // Filtering is done elsewhere
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            entries.Enqueue(new LogEntry(categoryName, logLevel, message, exception, clock.GetCurrentInstant()));
        }
    }

    private class LogEntry
    {
        private static readonly InstantPattern IsoMillisecondPattern = InstantPattern.CreateWithInvariantCulture("uuuu-MM-dd'T'HH:mm:ss.fff'Z'");

        public Instant Timestamp { get; }
        public string CategoryName { get; }
        public LogLevel LogLevel { get; }
        public Exception Exception { get; }
        public string Message { get; }

        internal LogEntry(string categoryName, LogLevel logLevel, string message, Exception exception, Instant timestamp) =>
            (CategoryName, LogLevel, Message, Exception, Timestamp) =
            (categoryName, logLevel, message, exception, timestamp);

        public override string ToString() => $"[{LogLevel}]: {Message}";

        internal string ToFileFormat()
        {
            var timestamp = IsoMillisecondPattern.Format(Timestamp);
            var exception = Exception is null ? "" : $"\r\n{Exception}";
            return $"{timestamp} [{CategoryName}] [{LogLevel}] {Message}{exception}";
        }
    }
}
