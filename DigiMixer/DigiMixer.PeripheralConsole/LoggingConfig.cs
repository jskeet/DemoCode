// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;

namespace DigiMixer.PeripheralConsole;

// Copied from JonSkeet.WpfLogging (and trimmed) so the same config can be used in both apps.
public class LoggingConfig
{
    public Dictionary<string, LogLevel> LogLevel { get; } = new Dictionary<string, LogLevel>();
}
