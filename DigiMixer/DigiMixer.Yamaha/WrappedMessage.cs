using DigiMixer.Yamaha.Core;

namespace DigiMixer.Yamaha;

/// <summary>
/// A wrapper for an original "raw" message, but with more semantics.
/// </summary>
public abstract class WrappedMessage(YamahaMessage rawMessage)
{
    public YamahaMessage RawMessage { get; } = rawMessage;

    public static WrappedMessage? TryParse(YamahaMessage message) =>
        (WrappedMessage?) SyncHashesMessage.TryParse(message) ??
        (WrappedMessage?) KeepAliveMessage.TryParse(message) ??
        SectionSchemaAndDataMessage.TryParse(message);
}
