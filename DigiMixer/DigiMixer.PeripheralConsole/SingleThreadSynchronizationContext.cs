using System.Collections.Concurrent;

namespace DigiMixer.PeripheralConsole;

// Taken from https://devblogs.microsoft.com/pfxteam/await-synchronizationcontext-and-console-apps/
// and https://devblogs.microsoft.com/pfxteam/await-synchronizationcontext-and-console-apps-part-3/
// with very small modifications.
internal sealed class SingleThreadSynchronizationContext :  SynchronizationContext
{
    private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> m_queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

    public override void Post(SendOrPostCallback d, object state) =>
        m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));

    public void RunOnCurrentThread()
    {
        while (m_queue.TryTake(out var workItem, Timeout.Infinite))
        {
            try
            {
                workItem.Key(workItem.Value);
            }
            catch (Exception e)
            {
                // TODO: use an ILogger or an event.
                Console.WriteLine($"Exception in synchronization context: {e}");
            }
        }
    }

    public void Complete() => m_queue.CompleteAdding();

    // From part 3...
    private int m_operationCount = 0;

    public override void OperationStarted()
    {
        Interlocked.Increment(ref m_operationCount);
    }

    public override void OperationCompleted()
    {
        if (Interlocked.Decrement(ref m_operationCount) == 0)
            Complete();
    }
}
