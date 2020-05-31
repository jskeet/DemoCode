// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using NodaTime;
using System;

namespace VDrumExplorer.ViewModel.Logging
{
    internal class LogEntry
    {
        internal Instant Timestamp { get; }
        internal string Message { get; }
        internal Exception? Exception { get; }
        internal LogLevel Level { get; }

        internal LogEntry(Instant timestamp, string message, LogLevel level, Exception? exception) =>
            (Timestamp, Message, Level, Exception) = (timestamp, message, level, exception);
    }
}
