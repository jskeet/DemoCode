// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using JonSkeet.CoreAppUtil;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace JonSkeet.WpfLogging;

/// <summary>
/// A memory-based log, consisting of log entries, suitable for viewing in <see cref="LogWindow"/>
/// or <see cref="LogViewerControl"/>.
/// </summary>
public sealed class MemoryLoggerProvider : ILoggerProvider, ILoggerFactory
{
    // A lock to protect "entries", which may be added to from multiple threads.
    private readonly object _lock = new object();
    private readonly List<LogEntry> entries;

    private readonly Instant start;
    private readonly IClock clock;
    private readonly Dispatcher dispatcher;
    private readonly LoggingConfig config;

    public event EventHandler<LogEntry> LogEntryLogged;

    public MemoryLoggerProvider(IClock clock, LoggingConfig config)
    {
        this.clock = clock;
        this.config = config;
        dispatcher = Dispatcher.CurrentDispatcher;
        start = clock.GetCurrentInstant();
        entries = new List<LogEntry>();
        if (config.UnbufferedLogging)
        {
            DateTime start = DateTime.UtcNow;
            var name = DeriveLogName();
            var logFile = Path.Combine(Path.GetTempPath(), $"{name}-{start:yyyyMMdd-HHmmssZ}.txt");
            LogEntryLogged += (sender, entry) =>
            {
                string entryText = entry.ToFileFormat() + "\r\n";
                File.AppendAllText(logFile, entryText);
            };
        }
    }

    public IReadOnlyList<LogEntry> GetAllLogEntries()
    {
        lock (_lock)
        {
            return entries.ToReadOnlyList();
        }
    }

    private LogLevel GetLogLevel(string category)
    {
        // TODO: Cache the result
        // TODO: Only include prefixes where there's a dot or full match (e.g. "Noda" shouldn't be a prefix for "NodaTime.Xyz")
        var matchingEntry = config.LogLevel
            .Where(entry => category.StartsWith(entry.Key, StringComparison.Ordinal))
            .OrderByDescending(entry => entry.Key.Length)
            .FirstOrDefault();
        return matchingEntry.Key is null
            ? config.LogLevel.GetValueOrDefault("Default", LogLevel.Debug)
            : matchingEntry.Value;
    }

    public void AddLogEntry(LogEntry log)
    {
        lock (_lock)
        {
            entries.Add(log);
        }
        if (dispatcher.Thread != Thread.CurrentThread)
        {
            dispatcher.BeginInvoke(() => LogEntryLogged?.Invoke(this, log));
            return;
        }
        LogEntryLogged?.Invoke(this, log);
    }

    /// <summary>
    /// Saves this log to the given directory, creating it if necessary and
    /// creating a log file containing a local timestamp. Any exceptions are swallowed.
    /// </summary>
    public void SaveToDirectory(string directory, string filePrefix)
    {
        try
        {
            var timestamp = DateTime.Now;
            Directory.CreateDirectory(directory);
            var logFile = Path.Combine(directory, $"{filePrefix}-{timestamp:yyyyMMdd-HHmmss}.txt");
            Save(logFile);
        }
        catch
        {
            // Just swallow exceptions at this point.
        }
    }

    public void Save(string file)
    {
        var allEntriesLocal = GetAllLogEntries();
        var formattedStart = InstantPattern.General.Format(start);
        var formattedEnd = InstantPattern.General.Format(clock.GetCurrentInstant());
        string[] prefix =
        {
            $"{DeriveLogName()} Log ({formattedStart} to {formattedEnd})",
            "----"
        };
        var lines = prefix.Concat(allEntriesLocal.Select(entry => entry.ToFileFormat()));
        File.WriteAllLines(file, lines);
    }

    private static string DeriveLogName()
    {
        string path = Environment.ProcessPath;
        return path is null ? "Unknown" : Path.GetFileNameWithoutExtension(path);
    }

    public ILogger CreateLogger(string categoryName) => new LoggerImpl(this, categoryName, GetLogLevel(categoryName));

    void IDisposable.Dispose()
    {
    }

    void ILoggerFactory.AddProvider(ILoggerProvider provider)
    {
        // We implement ILoggerFactory as a more suitable interface to inject into
        // other code, but really we only "want" the CreateLogger method.
    }

    private class LoggerImpl : ILogger
    {
        private readonly MemoryLoggerProvider log;
        private readonly string categoryName;
        private readonly LogLevel level;

        internal LoggerImpl(MemoryLoggerProvider log, string categoryName, LogLevel level) =>
            (this.log, this.categoryName, this.level) = (log, categoryName, level);

        public IDisposable BeginScope<TState>(TState state) => NoOpDisposable.Instance;

        // Explicitly disable ASP.NET Core per-request logging, due to it overwhelming things.
        // TODO: Do this in the logging configuration...
        public bool IsEnabled(LogLevel logLevel) => categoryName != "Microsoft.AspNetCore.Hosting.Diagnostics"
            && logLevel >= level;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            var message = formatter(state, exception);
            var entry = new LogEntry(categoryName, log.clock.GetCurrentInstant(), message, logLevel, exception);
            log.AddLogEntry(entry);
        }

        private class NoOpDisposable : IDisposable
        {
            internal static NoOpDisposable Instance { get; } = new NoOpDisposable();

            public void Dispose()
            {
            }
        }
    }
}
