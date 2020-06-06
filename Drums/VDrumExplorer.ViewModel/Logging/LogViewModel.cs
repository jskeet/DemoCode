// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VDrumExplorer.ViewModel.Logging
{
    // TODO: This feels more like a model...
    public class LogViewModel : ViewModelBase
    {
        private List<LogEntry> allLogEntries = new List<LogEntry>();
        public ObservableCollection<LogEntryViewModel> LogEntries { get; private set; }

        public ILogger Logger { get; }

        public IReadOnlyList<LogLevel> AllFilterLevels { get; } = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToList().AsReadOnly();

        private LogLevel filterLevel = LogLevel.Information;
        public LogLevel FilterLevel
        {
            get => filterLevel;
            set
            {
                if (SetProperty(ref filterLevel, value))
                {
                    // Changing the source is simpler than trying to handle the bulk update.
                    var matchingEntries = allLogEntries.Where(ShouldShowEntry).Select(entry => new LogEntryViewModel(entry));
                    LogEntries = new ObservableCollection<LogEntryViewModel>(matchingEntries);
                    RaisePropertyChanged(nameof(LogEntries));
                }
            }
        }

        private bool ShouldShowEntry(LogEntry entry) => entry.Level >= FilterLevel;

        public LogViewModel() : this(SystemClock.Instance)
        {
        }

        public LogViewModel(IClock clock)
        {
            LogEntries = new ObservableCollection<LogEntryViewModel>();
            Logger = new LoggerImpl(this, clock);
        }

        public void Clear()
        {
            LogEntries.Clear();
        }

        public void LogVersion(Type type)
        {
            var version = type.Assembly.GetCustomAttributes().OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            if (version != null)
            {
                Logger.LogInformation($"V-Drum Explorer version {version.InformationalVersion}");
            }
            else
            {
                Logger.LogInformation("Version attribute not found.");
            }
        }

        private void Log(LogEntry entry)
        {
            allLogEntries.Add(entry);
            if (ShouldShowEntry(entry))
            {
                LogEntries.Add(new LogEntryViewModel(entry));
            }
        }

        public void Save(string file)
        {
            var jsonEntries = allLogEntries.Select(entry => new JsonLogEntry(entry)).ToList();
            var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            string json = JsonConvert.SerializeObject(jsonEntries, Formatting.Indented, jsonSettings);
            File.WriteAllText(file, json);
        }

        private class LoggerImpl : ILogger
        {
            private readonly IClock clock;
            private readonly LogViewModel viewModel;

            internal LoggerImpl(LogViewModel viewModel, IClock clock) =>
                (this.viewModel, this.clock) = (viewModel, clock);

            public IDisposable BeginScope<TState>(TState state) => NoOpDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter(state, exception);
                var entry = new LogEntry(clock.GetCurrentInstant(), message, logLevel, exception);
                viewModel.Log(entry);
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
