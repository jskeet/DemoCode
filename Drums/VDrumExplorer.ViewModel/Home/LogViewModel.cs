// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace VDrumExplorer.ViewModel.Home
{
    public class LogViewModel
    {
        private readonly IClock clock;
        public ObservableCollection<LogEntry> LogEntries { get; }
        public ILogger Logger { get; }

        public LogViewModel() : this(SystemClock.Instance)
        {
        }

        public LogViewModel(IClock clock)
        {
            LogEntries = new ObservableCollection<LogEntry>();
            this.clock = clock;
            Logger = new LoggerImpl(this);
        }

        public void Log(string text)
        {
            var entry = new LogEntry(clock.GetCurrentInstant(), text);
            LogEntries.Add(entry);
        }

        public void Log(string text, Exception exception)
        {
            // TODO: Keep the exception in the log entry?
            Log($"{text}: {exception.GetType().Name}: {exception.Message}");
        }

        public void Clear()
        {
            LogEntries.Clear();
        }

        public void Save(string file) =>
            File.WriteAllLines(file, LogEntries.Select(entry => $"{entry.Timestamp:yyyy-MM-ddTHH:mm:ss} {entry.Text}"));

        private class LoggerImpl : ILogger
        {
            private readonly LogViewModel viewModel;

            internal LoggerImpl(LogViewModel viewModel) =>
                this.viewModel = viewModel;

            public IDisposable BeginScope<TState>(TState state) => NoOpDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                if (exception is null)
                {
                    viewModel.Log(message);
                }
                else
                {
                    viewModel.Log(message, exception);
                }
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
