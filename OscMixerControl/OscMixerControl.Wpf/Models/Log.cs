// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace OscMixerControl.Wpf.Models
{
    /// <summary>
    /// A log, consisting of log entries.
    /// </summary>
    public class Log
    {
        public ILogger Logger { get; }

        public IReadOnlyList<LogEntry> AllEntries { get; }
        public event EventHandler<LogEntry> LogEntryLogged;

        private List<LogEntry> entries;

        public Log(IClock clock)
        {
            entries = new List<LogEntry>();
            Logger = new LoggerImpl(this, clock);
            AllEntries = new ReadOnlyCollection<LogEntry>(entries);
        }

        public void AddLogEntry(LogEntry log)
        {
            entries.Add(log);
            LogEntryLogged?.Invoke(this, log);
        }


        public void Save(string file)
        {
            string[] prefix = { "OSC Mixer Log", "----" };
            var lines = prefix.Concat(AllEntries.Select(FormatLogEntry));
            File.WriteAllLines(file, lines);

            string FormatLogEntry(LogEntry entry)
            {
                var timestamp = InstantPattern.General.Format(entry.Timestamp);
                var exception = entry.Exception is null
                    ? ""
                    : $"\r\n{entry.Exception}";
                return $"{timestamp} [{entry.Level}] {entry.Message}{exception}";
            }
        }

        private class LoggerImpl : ILogger
        {
            private readonly IClock clock;
            private readonly Log log;

            internal LoggerImpl(Log log, IClock clock) =>
                (this.log, this.clock) = (log, clock);

            public IDisposable BeginScope<TState>(TState state) => NoOpDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                var entry = new LogEntry(clock.GetCurrentInstant(), message, logLevel, exception);
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
}
