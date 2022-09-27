namespace DigiMixer;

// FIXME: Expected thread safety?

/// <summary>
/// Interface implemented by different protocols, e.g. OSC etc.
/// </summary>
public interface IMixerApi
{
    /// <summary>
    /// Registers the given receiver for updates to the state of the mixer.
    /// </summary>
    /// <param name="receiver">The receiver to register.</param>
    void RegisterReceiver(IMixerReceiver receiver);

    /// <summary>
    /// Sets the fader level for the given input/output channel combination.
    /// </summary>
    /// <param name="inputId">The input channel to change the fader level for.</param>
    /// <param name="outputId">The output channel to change the fader level for.</param>
    Task SetFaderLevel(InputChannelId inputId, OutputChannelId outputId, FaderLevel level);

    /// <summary>
    /// Mutes or unmutes the given input channel.
    /// </summary>
    /// <param name="inputId">The input channel to mute/unmute.</param>
    Task SetMuted(InputChannelId inputId, bool muted);

    /// <summary>
    /// Mutes or unmutes the given output channel.
    /// </summary>
    /// <param name="outputId">The output channel to mute/unmute.</param>
    Task SetMuted(OutputChannelId outputId, bool muted);

    /// <summary>
    /// Requests the name of the given input channel. The result will
    /// be sent via <see cref="IMixerReceiver.ReceiveChannelName(InputChannelId, string)"/>
    /// </summary>
    /// <param name="inputId">The input channel to request the name of.</param>
    Task RequestName(InputChannelId inputId);

    /// <summary>
    /// Requests the name of the given output channel. The result will
    /// be sent via <see cref="IMixerReceiver.ReceiveChannelName(OutputChannelId, string)"/>
    /// </summary>
    /// <param name="outputId">The output channel to request the name of.</param>
    Task RequestName(OutputChannelId outputId);

    /// <summary>
    /// Requests the mute status of the given input channel. The result will
    /// be sent via <see cref="IMixerReceiver.ReceiveMuteStatus(InputChannelId, bool)"/>
    /// </summary>
    /// <param name="outputId">The input channel to request the mute status of.</param>
    Task RequestMuteStatus(InputChannelId inputId);

    /// <summary>
    /// Requests the mute status of the given output channel. The result will
    /// be sent via <see cref="IMixerReceiver.ReceiveMuteStatus(OutputChannelId, bool)"/>
    /// </summary>
    /// <param name="outputId">The output channel to request the mute status of.</param>
    Task RequestMuteStatus(OutputChannelId outputId);

    /// <summary>
    /// Requests the fader level of the given input/output channel combination.
    /// </summary>
    /// <param name="inputId">The input channel to request the fader level for.</param>
    /// <param name="outputId">The output channel to request the fader level for.</param>
    Task RequestFaderLevel(InputChannelId inputId, OutputChannelId outputId);

    /// <summary>
    /// Request that updates are provided for channel values.
    /// This must be called periodically - TODO: clarify that.
    /// </summary>
    Task RequestChannelUpdates();

    /// <summary>
    /// Requests that meter levels are regularly reported.
    /// This must be called periodically - TODO: clarify that.
    /// TODO: Introduce a "meter frequency" enum? Do Ui protocol and A&H support this?
    /// </summary>
    Task RequestMeterUpdates();
}
