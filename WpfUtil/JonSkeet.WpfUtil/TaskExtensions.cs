// Copyright 2014 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;

namespace JonSkeet.WpfUtil;

public static class TaskExtensions
{
    /// <summary>
    /// Ignores the input. This is handy for "fire and forget" calls where we
    /// really don't want to await the task.
    /// </summary>
    public static void Ignore(this Task task, ILogger logger = null)
    {
        task.ContinueWith(t => logger?.LogError(t.Exception, "Ignored error"), TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// Returns a task that will complete when the original task is either completed or canceled,
    /// but without an exception in the case of cancellation.
    /// </summary>
    public static async Task IgnoreCancellation(this Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}
