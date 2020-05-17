// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

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

        public LogViewModel() : this(SystemClock.Instance)
        {
        }

        public LogViewModel(IClock clock)
        {
            LogEntries = new ObservableCollection<LogEntry>();
            this.clock = clock;
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
    }
}
