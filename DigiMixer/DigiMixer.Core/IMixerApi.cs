namespace DigiMixer.Core;

// FIXME: Expected thread safety? Cancellation tokens?

/// <summary>
/// Interface implemented by different protocols, e.g. OSC etc.
/// </summary>
public interface IMixerApi : IDisposable
{
    // TODO: How do we deregister? Should we have another method?

    /// <summary>
    /// Registers the given receiver for updates to the state of the mixer.
    /// </summary>
    /// <param name="receiver">The receiver to register.</param>
    void RegisterReceiver(IMixerReceiver receiver);

    /// <summary>
    /// Performs any initial connection required. This is separated from construction so that
    /// receivers may be registered before the initial connection is performed. Any previous connection
    /// should be disconnected. This method should not return without definitely establishing a connection.
    /// </summary>
    Task Connect(CancellationToken cancellationToken);

    /// <summary>
    /// Detects the supported channel configuration of the mixer (including stereo links).
    /// Any already-registered receivers may receive additional information as part of this detection.
    /// </summary>
    /// <returns>A task representing the detected channel configuration.</returns>
    Task<MixerChannelConfiguration> DetectConfiguration(CancellationToken cancellationToken);

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
    /// Sends any keep-alive messages.
    /// </summary>
    Task SendKeepAlive();

    /// <summary>
    /// Checks the connection status, returning true for a healthy connection
    /// or false if the mixer is unavailable. A faulted or cancelled task also represents
    /// an unavailable connection.
    /// </summary>
    Task<bool> CheckConnection(CancellationToken cancellationToken);

    /// <summary>
    /// The interval at which keep-alive messages should be sent.
    /// </summary>
    TimeSpan KeepAliveInterval { get; }
}
