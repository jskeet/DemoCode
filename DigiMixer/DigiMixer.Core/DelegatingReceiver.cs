using System.Collections.Concurrent;

namespace DigiMixer.Core;

/// <summary>
/// Receiver which allows further receivers to be registered with it.
/// This allows each <see cref="IMixerApi"/> implementation to
/// use a single instance of this class, and register additional
/// receivers with <see cref="RegisterReceiver(DigiMixer.Core.IMixerReceiver)"/>.
/// </summary>
public sealed class DelegatingReceiver : IMixerReceiver
{
    // Note: an alternative implementation might only use ConcurrentBag when there's
    // more than one receiver. Commonly there's only one, so creating a collection
    // snapshot on each call is over-the-top.
    private readonly ConcurrentBag<IMixerReceiver> receivers = new ConcurrentBag<IMixerReceiver>();

    public bool IsEmpty => receivers.IsEmpty;

    public void RegisterReceiver(IMixerReceiver receiver) =>
        receivers.Add(receiver);

    public void ReceiveFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level)
    {
        foreach (var receiver in receivers)
        {
            receiver.ReceiveFaderLevel(inputId, outputId, level);
        }
    }

    public void ReceiveFaderLevel(ChannelId outputId, FaderLevel level)
    {
        foreach (var receiver in receivers)
        {
            receiver.ReceiveFaderLevel(outputId, level);
        }
    }

    public void ReceiveMeterLevels((ChannelId channelId, MeterLevel level)[] levels)
    {
        foreach (var receiver in receivers)
        {
            receiver.ReceiveMeterLevels(levels);
        }
    }

    public void ReceiveChannelName(ChannelId channelId, string? name)
    {
        foreach (var receiver in receivers)
        {
            receiver.ReceiveChannelName(channelId, name);
        }
    }

    public void ReceiveMuteStatus(ChannelId channelId, bool muted)
    {
        foreach (var receiver in receivers)
        {
            receiver.ReceiveMuteStatus(channelId, muted);
        }
    }

    public void ReceiveMixerInfo(MixerInfo info)
    {
        foreach (var receiver in receivers)
        {
            receiver.ReceiveMixerInfo(info);
        }
    }
}
