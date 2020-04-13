// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime;
using System.Collections.ObjectModel;

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

        public void Clear()
        {
            LogEntries.Clear();
        }
    }
}
