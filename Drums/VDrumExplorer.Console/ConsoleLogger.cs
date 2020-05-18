// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.CommandLine.IO;

namespace VDrumExplorer.Console
{
    internal sealed class ConsoleLogger : ILogger
    {
        private readonly IStandardStreamWriter writer;

        internal ConsoleLogger(IStandardStreamWriter writer) =>
            this.writer = writer;

        public IDisposable BeginScope<TState>(TState state) => NoOpDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        private void Log(string text) =>
            writer.WriteLine(text);

        private void Log(string message, Exception e)
        {
            // TODO: Aggregate exception etc.
            Log($"{message}: {e}");
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            if (exception is null)
            {
                Log(message);
            }
            else
            {
                Log(message, exception);
            }
        }

        private class NoOpDisposable : IDisposable
        {
            internal static NoOpDisposable Instance { get; } = new NoOpDisposable();

            public void Dispose()
            {
            }
        }
    }
}
