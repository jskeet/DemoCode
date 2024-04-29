namespace DigiMixer.PeripheralConsole;

// Taken from https://devblogs.microsoft.com/pfxteam/await-synchronizationcontext-and-console-apps/
// and https://devblogs.microsoft.com/pfxteam/await-synchronizationcontext-and-console-apps-part-3/
internal class AsyncPump
{
    public static void Run(Func<Task> func)
    {
        var prevCtx = SynchronizationContext.Current;
        try
        {
            var syncCtx = new SingleThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncCtx);
            var t = func();
            t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);
            syncCtx.RunOnCurrentThread();
            t.GetAwaiter().GetResult();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prevCtx);
        }
    }
}
