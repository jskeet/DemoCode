using System.Collections.Concurrent;

namespace DigiMixer.PeripheralConsole;

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
                Console.WriteLine($"Exception in synchronization context: {e}");
            }
        }
    }

    public void Complete() => m_queue.CompleteAdding();
}
