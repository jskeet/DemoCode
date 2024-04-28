// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;

namespace JonSkeet.CoreAppUtil;

public static class Disposables
{
    public static IDisposable ForAction(Action action) => new ActionDisposable(action);

    private class ActionDisposable : IDisposable
    {
        private readonly Action action;

        public ActionDisposable(Action action) =>
            this.action = action;

        public void Dispose() => action();
    }

    public static void DisposeWithCatch(this IDisposable disposable, ILogger logger)
    {
        try
        {
            disposable?.Dispose();
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Error while disposing object of type '{type}'", disposable.GetType().Name);
        }
    }

    public static async ValueTask DisposeAsyncWithCatch(this IAsyncDisposable disposable, ILogger logger)
    {
        try
        {
            await (disposable?.DisposeAsync() ?? ValueTask.CompletedTask);
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Error while disposing object of type '{type}'", disposable.GetType().Name);
        }
    }

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2) =>
        ForAction(() =>
        {
            disposable1.Dispose();
            disposable2.Dispose();
        });

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3) =>
        ForAction(() =>
        {
            disposable1.Dispose();
            disposable2.Dispose();
            disposable3.Dispose();
        });

    /// <summary>
    /// Equivalent to <see cref="DisposeWithCatch(IDisposable, ILogger)"/> but for
    /// a disposal action represented by an <see cref="Action"/>.
    /// </summary>
    public static void DisposeWithCatch(Action action, ILogger logger)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            var method = action.Method;
            logger?.LogError(e, "Error while executing disposal action '{method}'", $"{method.DeclaringType.Name}.{method.Name}");
        }
    }

    public static void DisposeAllWithCatch(IEnumerable<IDisposable> disposables, ILogger logger)
    {
        foreach (var disposable in disposables)
        {
            DisposeWithCatch(disposable, logger);
        }
    }

    public static async ValueTask DisposeAllAsyncWithCatch(IEnumerable<IAsyncDisposable> disposables, ILogger logger)
    {
        foreach (var disposable in disposables)
        {
            await DisposeAsyncWithCatch(disposable, logger);
        }
    }
}
