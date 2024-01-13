// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;

namespace JonSkeet.WpfLogging;

public sealed class LogEntryViewModel
{
    private static readonly DateTimeZone TimeZone = DateTimeZoneProviders.Bcl.GetSystemDefault();
    private static readonly LocalDateTimePattern TimestampPattern = LocalDateTimePattern.CreateWithInvariantCulture("HH:mm:ss.ffffff");

    internal LogEntry Entry { get; }

    public string CategoryName => Entry.CategoryName;
    public string ToolTip { get; }
    public string Timestamp { get; }
    public string LevelText => FormatLevel(Entry.Level);
    public LogLevel Level => Entry.Level;
    public string Text { get; }

    internal LogEntryViewModel(LogEntry logEntry)
    {
        Entry = logEntry;
        Timestamp = TimestampPattern.Format(logEntry.Timestamp.InZone(TimeZone).LocalDateTime);
        Text = Entry.Exception is null ? Entry.Message : $"{Entry.Message}: {Entry.Exception.GetType()}; {Entry.Exception.Message}";
        ToolTip = Entry.Exception?.StackTrace;
    }

    private static string FormatLevel(LogLevel level) => level switch
    {
        LogLevel.Trace => "Trace",
        LogLevel.Debug => "Debug",
        LogLevel.Information => "Info",
        LogLevel.Warning => "Warn",
        LogLevel.Error => "Error",
        LogLevel.Critical => "Critical",
        LogLevel.None => "None",
        _ => "Unknown"
    };
}
