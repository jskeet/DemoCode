using DigiMixer.DmSeries.Core;

namespace DigiMixer.DmSeries;

/// <summary>
/// Base class for type-specific messages. These wrap <see cref="CqRawMessage"/>.
/// </summary>
public abstract class DmMessage
{
    internal const string MproType = "MPRO";

    public DmRawMessage RawMessage { get; }
    public ReadOnlySpan<byte> Data => RawMessage.Data;
    public string Type => RawMessage.Type;

    protected DmMessage(DmRawMessage rawMessage)
    {
        RawMessage = rawMessage;
    }

    public static DmMessage FromRawMessage(DmRawMessage rawMessage) => rawMessage.Type switch
    {
        MproType => new DmMproMessage(rawMessage),
         _ => new DmUnknownMessage(rawMessage)
    };

    public override string ToString() => RawMessage.ToString();
}
