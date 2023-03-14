namespace DigiMixer;

// TODO: Should this be immutable? Use a record?

/// <summary>
/// Timing details for connection handling.
/// </summary>
public record ConnectionTiming
{
    // TODO: Should the API itself control this?
    /// <summary>
    /// Interval between keep-alive calls. Defaults to 3 seconds.
    /// </summary>
    public TimeSpan KeepAliveInterval { get; init; } = TimeSpan.FromSeconds(3);

    /// <summary>
    /// How long the keep-alive is permitted to take before being cancelled.
    /// Defaults to 2 seconds.
    /// </summary>
    public TimeSpan KeepAliveTimeout { get; init; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How long to wait between attempts to reconnect after a failure
    /// and then <see cref="InitialReconnectionAttempts"/> attempts.
    /// Defaults to 10 seconds.
    /// </summary>
    public TimeSpan EventualReconnectionInterval { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How many times the mixer should attempt to reconnect initially
    /// after a failure, waiting <see cref="InitialReconnectionInterval"/> between
    /// attempts. Defaults to 5.
    /// </summary>
    public int InitialReconnectionAttempts { get; init; } = 5;

    /// <summary>
    /// How long to wait between attempts to reconnect initially,
    /// straight after a failure. Defaults to 1 second.
    /// </summary>
    public TimeSpan InitialReconnectionInterval { get; init; } = TimeSpan.FromSeconds(1);
}
