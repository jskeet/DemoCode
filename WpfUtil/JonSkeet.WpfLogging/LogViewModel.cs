// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using JonSkeet.WpfUtil;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JonSkeet.WpfLogging;

public class LogViewModel : ViewModelBase
{
    private const int DisplayedLogLimit = 250;

    private readonly MemoryLoggerProvider log;

    public ObservableCollection<LogEntryViewModel> LogEntries { get; private set; }

    public IReadOnlyList<LogLevel> AllFilterLevels { get; } = Enum.GetValues<LogLevel>().ToReadOnlyList();

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

    public LogViewModel(MemoryLoggerProvider log)
    {
        this.log = log;
        log.LogEntryLogged += MaybeAddLogEntry;
        PopulateLogEntries();
    }

    private void PopulateLogEntries()
    {
        // Changing the source is simpler than trying to handle the bulk update.
        // It's just possible that we miss some log entries being added to the source while we're doing this,
        // but such is life.
        var matchingEntries = log.GetAllLogEntries().Where(ShouldShowEntry).OrderBy(entry => entry.Timestamp).Select(entry => new LogEntryViewModel(entry)).ToList();
        if (matchingEntries.Count > DisplayedLogLimit)
        {
            matchingEntries = matchingEntries.Skip(matchingEntries.Count - DisplayedLogLimit).ToList();
        }
        LogEntries = new ObservableCollection<LogEntryViewModel>(matchingEntries);
        RaisePropertyChanged(nameof(LogEntries));
    }

    private void MaybeAddLogEntry(object sender, LogEntry entry)
    {
        if (ShouldShowEntry(entry))
        {
            // Make sure we add the entries in chronological order
            int index = LogEntries.Count - 1;
            while (index >= 0 && LogEntries[index].Entry.Timestamp >= entry.Timestamp)
            {
                index--;
            }
            var vm = new LogEntryViewModel(entry);
            int insertionIndex = index + 1;
            if (insertionIndex == LogEntries.Count)
            {
                LogEntries.Add(vm);
            }
            else
            {
                LogEntries.Insert(insertionIndex, vm);
            }

            while (LogEntries.Count > DisplayedLogLimit)
            {
                LogEntries.RemoveAt(0);
            }
        }
    }

    private bool ShouldShowEntry(LogEntry entry) => entry.Level >= FilterLevel;

    public void Clear()
    {
        LogEntries.Clear();
    }
}
