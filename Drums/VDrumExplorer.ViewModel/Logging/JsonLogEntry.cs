// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using System;

namespace VDrumExplorer.ViewModel.Logging
{
    internal sealed class JsonLogEntry
    {
        [JsonProperty("timestamp")]
        internal Instant Timestamp { get; }

        [JsonProperty("level")]
        [JsonConverter(typeof(StringEnumConverter))]
        internal LogLevel Level { get; }

        [JsonProperty("message")]
        internal string Message { get; }

        [JsonProperty("exception")]
        internal JsonException? Exception { get; }

        internal JsonLogEntry(LogEntry entry) =>
            (Timestamp, Level, Message, Exception) =
            (entry.Timestamp, entry.Level, entry.Message, entry.Exception is null ? null : new JsonException(entry.Exception));

        internal sealed class JsonException
        {
            [JsonProperty("message")]
            internal string Message { get; }

            [JsonProperty("type")]
            internal string Type { get; }

            [JsonProperty("stackTrace")]
            internal string? StackTrace { get; }

            [JsonProperty("innertException")]
            internal JsonException? InnerException { get; }

            internal JsonException(Exception e) =>
                (Message, Type, StackTrace, InnerException) =
                (e.Message, e.GetType().Name, e.StackTrace, e.InnerException is null ? null : new JsonException(e.InnerException));
        }
    }
}
