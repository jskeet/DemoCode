// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using OscMixerControl.Wpf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OscMixerControl.Wpf.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private readonly Log log;

        public ObservableCollection<LogEntryViewModel> LogEntries { get; private set; }

        public IReadOnlyList<LogLevel> AllFilterLevels { get; } = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToList().AsReadOnly();

        private string manualEntryText;
        public string ManualEntryText
        {
            get => manualEntryText;
            set => SetProperty(ref manualEntryText, value);
        }

        private LogLevel filterLevel = LogLevel.Information;
        public LogLevel FilterLevel
        {
            get => filterLevel;
            set
            {
                if (SetProperty(ref filterLevel, value))
                {
                    PopulateLogEntries();
                }
            }
        }

        public LogViewModel(Log log)
        {
            this.log = log;
            log.LogEntryLogged += MaybeAddLogEntry;
            PopulateLogEntries();
        }

        private void PopulateLogEntries()
        {
            // Changing the source is simpler than trying to handle the bulk update.
            var matchingEntries = log.AllEntries.Where(ShouldShowEntry).Select(entry => new LogEntryViewModel(entry));
            LogEntries = new ObservableCollection<LogEntryViewModel>(matchingEntries);
            RaisePropertyChanged(nameof(LogEntries));
        }

        private void MaybeAddLogEntry(object sender, LogEntry entry)
        {
            if (ShouldShowEntry(entry))
            {
                LogEntries.Add(new LogEntryViewModel(entry));
            }
        }

        private bool ShouldShowEntry(LogEntry entry) => entry.Level >= FilterLevel;

        public void Clear()
        {
            LogEntries.Clear();
        }
    }
}
