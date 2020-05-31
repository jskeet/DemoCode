// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;

namespace VDrumExplorer.ViewModel.Logging
{
    public sealed class LogEntryViewModel
    {
        private static readonly DateTimeZone TimeZone = DateTimeZoneProviders.Bcl.GetSystemDefault();
        private static readonly LocalDateTimePattern TimestampPattern = LocalDateTimePattern.CreateWithInvariantCulture("HH:mm:ss.fff");

        internal LogEntry Entry { get; }

        public string? ToolTip { get; }
        public string Timestamp { get; }
        public LogLevel Level => Entry.Level;
        public string Text { get; }

        internal LogEntryViewModel(LogEntry logEntry)
        {
            Entry = logEntry;
            Timestamp = TimestampPattern.Format(logEntry.Timestamp.InZone(TimeZone).LocalDateTime);
            Text = Entry.Exception is null ? Entry.Message : $"{Entry.Message}: {Entry.Exception.GetType()}; {Entry.Exception.Message}";
            ToolTip = Entry.Exception is null ? null : Entry.Exception.StackTrace;
        }
    }
}
