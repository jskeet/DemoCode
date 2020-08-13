// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace VDrumExplorer.Utility
{
    /// <summary>
    /// Methods to help with performance measurements.
    /// </summary>
    public static class Timing
    {
        public static void LogTiming(ILogger logger, string description, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            logger.LogDebug($"{description} in {(int) stopwatch.ElapsedMilliseconds}ms");
        }

        public static T LogTiming<T>(ILogger logger, string description, Func<T> func)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = func();
            stopwatch.Stop();
            logger.LogDebug($"{description} in {(int) stopwatch.ElapsedMilliseconds}ms");
            return result;
        }

        public static void DebugConsoleLogTiming(string description, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            Console.WriteLine($"{description} in {(int) stopwatch.ElapsedMilliseconds}ms");
            Debug.WriteLine($"{description} in {(int) stopwatch.ElapsedMilliseconds}ms");
        }

        public static T DebugConsoleLogTiming<T>(string description, Func<T> func)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = func();
            stopwatch.Stop();
            Console.WriteLine($"{description} in {(int) stopwatch.ElapsedMilliseconds}ms");
            Debug.WriteLine($"{description} in {(int) stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
}
