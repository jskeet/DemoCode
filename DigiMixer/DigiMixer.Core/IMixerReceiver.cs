namespace DigiMixer.Core;

/// <summary>
/// Interface to receive information from a receiver.
/// This is registered with an <see cref="IMixerApi"/>.
/// </summary>
public interface IMixerReceiver
{
    void ReceiveFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level);
    void ReceiveFaderLevel(ChannelId outputId, FaderLevel level);
    void ReceiveMeterLevels((ChannelId channelId, MeterLevel level)[] levels);
    void ReceiveChannelName(ChannelId channelId, string name);
    void ReceiveMuteStatus(ChannelId channelId, bool muted);
    void ReceiveMixerInfo(MixerInfo info);
}
