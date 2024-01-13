// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using System;

namespace JonSkeet.WpfLogging;

public class LogEntry
{
    private static readonly InstantPattern IsoMillisecondPattern = InstantPattern.CreateWithInvariantCulture("uuuu-MM-dd'T'HH:mm:ss.fff'Z'");

    public string CategoryName { get; }
    public Instant Timestamp { get; }
    public string Message { get; }
    public Exception Exception { get; }
    public LogLevel Level { get; }

    internal LogEntry(string categoryName, Instant timestamp, string message, LogLevel level, Exception exception) =>
        (CategoryName, Timestamp, Message, Level, Exception) = (categoryName, timestamp, message, level, exception);

    internal string ToFileFormat()
    {
        var timestamp = IsoMillisecondPattern.Format(Timestamp);
        var exception = Exception is null ? "" : $"\r\n{Exception}";
        return $"{timestamp} [{CategoryName}] [{Level}] {Message}{exception}";
    }
}
