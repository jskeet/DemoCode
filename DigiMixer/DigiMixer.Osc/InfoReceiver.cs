using OscCore;
using OscMixerControl;
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
    private readonly TaskCompletionSource<IDictionary<string, OscMessage>> source;

    private InfoReceiver(string[] addresses)
    {
        this.addresses = addresses.ToHashSet();
        remainingValues = this.addresses.Count;
        source = new TaskCompletionSource<IDictionary<string, OscMessage>>();
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
    /// </summary>
    internal static async Task<IDictionary<string, OscMessage>> RequestAndWait(IOscClient client, CancellationToken cancellationToken, params string[] addresses)
    {
        var receiver = new InfoReceiver(addresses);
        client.PacketReceived += receiver.Receive;

        using var _ = cancellationToken.Register(() => receiver.source.TrySetCanceled());
        try
        {
            foreach (var address in addresses)
            {
                await client.SendAsync(new OscMessage(address)).ConfigureAwait(false);
            }
            return await receiver.source.Task.ConfigureAwait(false);
        }
        finally
        {
            client.PacketReceived -= receiver.Receive;
        }
    }
}
