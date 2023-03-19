namespace DigiMixer.Core;

/// <summary>
/// Common controller statuses.
/// </summary>
public enum ControllerStatus
{
    NotConnected = 0,
    Connected = 1,
    Running = 2,
    Faulted = 3,
    Disposed = 4
}
