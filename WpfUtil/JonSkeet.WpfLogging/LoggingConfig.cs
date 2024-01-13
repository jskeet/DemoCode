// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace JonSkeet.WpfLogging;

public class LoggingConfig
{
    public Dictionary<string, LogLevel> LogLevel { get; } = new Dictionary<string, LogLevel>();

    /// <summary>
    /// When true, all log entries are written to disk immediately.
    /// This is inefficient, but useful when diagnosing errors that cause abrupt process terminations.
    /// </summary>
    public bool UnbufferedLogging { get; set; }
}
