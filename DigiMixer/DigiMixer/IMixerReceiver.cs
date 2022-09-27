namespace DigiMixer;

/// <summary>
/// Interface to receive information from a receiver.
/// This is registered with an <see cref="IMixerApi"/>.
/// </summary>
public interface IMixerReceiver
{
    void ReceiveFaderLevel(InputChannelId inputId, OutputChannelId outputId, FaderLevel level);
    void ReceiveMeterLevel(OutputChannelId outputId, MeterLevel level);
    void ReceiveMeterLevel(InputChannelId outputId, MeterLevel level);
    void ReceiveChannelName(InputChannelId channelId, string name);
    void ReceiveChannelName(OutputChannelId channelId, string name);
    void ReceiveMuteStatus(InputChannelId channelId, bool muted);
    void ReceiveMuteStatus(OutputChannelId channelId, bool muted);
}
