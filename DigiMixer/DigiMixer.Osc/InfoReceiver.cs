using OscCore;
using System.Collections.Concurrent;

namespace DigiMixer.Osc;

/// <summary>
/// Convenience code to send requests for information about given OSC addresses,
/// and return the information when it's all been received (or time out via a cancellation token).
/// </summary>
internal sealed class InfoReceiver
{
    private ConcurrentDictionary<string, OscMessage> messages;
    private readonly HashSet<string> addresses;
    private int remainingValues;
    private readonly TaskCompletionSource<IDictionary<string, OscMessage>?> source;

    private InfoReceiver(string[] addresses)
    {
        this.addresses = addresses.ToHashSet();
        remainingValues = this.addresses.Count;
        source = new TaskCompletionSource<IDictionary<string, OscMessage>?>();
        messages = new ConcurrentDictionary<string, OscMessage>();
    }

    void Receive(object? sender, OscPacket e)
    {
        if (e is not OscMessage message || !addresses.Contains(message.Address))
        {
            return;
        }
        var added = messages.TryAdd(message.Address, message);
        if (added)
        {
            if (Interlocked.Decrement(ref remainingValues) == 0)
            {
                source.TrySetResult(messages);
            }
        }
    }

    /// <summary>
    /// Requests values for the given set of addresses, returning them as a dictionary of messages when all have been received.
    /// If the request is cancelled via the cancellation token, a null reference is returned (and no exception is thrown).
    /// </summary>
    internal static Task<IDictionary<string, OscMessage>?> RequestAndWait(IOscClient client, CancellationToken cancellationToken, params string[] addresses) =>
        RequestAndWait(client, cancellationToken, addresses.Select(addr => new OscMessage(addr)), addresses);

    /// <summary>
    /// Sends the given messages to request a refresh for the given addresses, then waits for them all to be received,
    /// returning them as a dictionary of messages when all have been received.
    /// </summary>
    internal static async Task<IDictionary<string, OscMessage>?> RequestAndWait(IOscClient client, CancellationToken cancellationToken, IEnumerable<OscMessage> messages, params string[] addresses)
    {
        var receiver = new InfoReceiver(addresses);
        client.PacketReceived += receiver.Receive;

        using var _ = cancellationToken.Register(() => receiver.source.TrySetResult(null));
        try
        {
            foreach (var message in messages)
            {
                await client.SendAsync(message);
            }
            return await receiver.source.Task.ConfigureAwait(false);
        }
        finally
        {
            client.PacketReceived -= receiver.Receive;
        }
    }
}
