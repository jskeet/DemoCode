// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;

namespace VDrumExplorer.Blazor
{
    internal class SimpleLogger : ILogger
    {
        private Action<string> action;

        public SimpleLogger(Action<string> action) => this.action = action;

        public IDisposable BeginScope<TState>(TState state) => NoOpDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            action(formatter(state, exception));
        }

        private class NoOpDisposable : IDisposable
        {
            internal static NoOpDisposable Instance { get; } = new NoOpDisposable();
            private NoOpDisposable() { }

            public void Dispose() { }
        }
    }
}
