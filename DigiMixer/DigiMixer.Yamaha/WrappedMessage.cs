using DigiMixer.Yamaha.Core;

namespace DigiMixer.Yamaha;

/// <summary>
/// A wrapper for an original "raw" message, but with more semantics.
/// </summary>
public abstract class WrappedMessage(YamahaMessage rawMessage)
{
    public YamahaMessage RawMessage { get; } = rawMessage;

    public static WrappedMessage? TryParse(YamahaMessage message) =>
        SyncHashesMessage.TryParse(message) ??
        KeepAliveMessage.TryParse(message) ??
        SectionSchemaAndDataMessage.TryParse(message) ??
        MonitorDataMessage.TryParse(message) ??
        (WrappedMessage?) SingleValueMessage.TryParse(message);
}
