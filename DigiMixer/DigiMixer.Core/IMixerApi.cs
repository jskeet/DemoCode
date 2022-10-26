namespace DigiMixer.Core;

// FIXME: Expected thread safety?

/// <summary>
/// Interface implemented by different protocols, e.g. OSC etc.
/// </summary>
public interface IMixerApi : IDisposable
{
    /// <summary>
    /// Registers the given receiver for updates to the state of the mixer.
    /// </summary>
    /// <param name="receiver">The receiver to register.</param>
    void RegisterReceiver(IMixerReceiver receiver);

    /// <summary>
    /// Performs any initial connection required. This is separated from construction so that
    /// receivers may be registered before the initial connection is performed. Any previous connection
    /// should be disconnected.
    /// </summary>
    Task Connect();

    /// <summary>
    /// Detects the supported channel configuration of the mixer (including stereo links).
    /// Any already-registered receivers may receive additional information as part of this detection.
    /// </summary>
    /// <returns>A task representing the detected channel configuration.</returns>
    Task<MixerChannelConfiguration> DetectConfiguration();

    /// <summary>
    /// Requests all mixer data. Where channels are known, <paramref name="channelIds"/>
    /// can be used to specify which channels are of interest.
    /// </summary>
    /// <param name="channelIds">The channels to request data for.</param>
    Task RequestAllData(IReadOnlyList<ChannelId> channelIds);

    /// <summary>
    /// Sets the fader level for the given input/output channel combination.
    /// </summary>
    /// <param name="inputId">The input channel to change the fader level for.</param>
    /// <param name="outputId">The output channel to change the fader level for.</param>
    Task SetFaderLevel(ChannelId inputId, ChannelId outputId, FaderLevel level);

    /// <summary>
    /// Sets the fader level for the given overall output channel.
    /// </summary>
    /// <param name="outputId">The output channel to change the fader level for.</param>
    Task SetFaderLevel(ChannelId outputId, FaderLevel level);

    /// <summary>
    /// Mutes or unmutes the given channel.
    /// </summary>
    /// <param name="channelId">The channel to mute/unmute.</param>
    Task SetMuted(ChannelId channelId, bool muted);

    /// <summary>
    /// Sends any keep-alive messages. This should be called roughly once every 3 seconds.
    /// </summary>
    Task SendKeepAlive();
}
