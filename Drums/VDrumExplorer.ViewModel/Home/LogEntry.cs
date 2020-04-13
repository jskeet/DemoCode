// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime;

namespace VDrumExplorer.ViewModel.Home
{
    public class LogEntry
    {
        public Instant Timestamp { get; }
        public string Text { get; }

        public LogEntry(Instant timestamp, string text) =>
            (Timestamp, Text) = (timestamp, text);
    }
}
