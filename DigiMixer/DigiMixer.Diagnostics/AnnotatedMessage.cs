using DigiMixer.Core;
using System.Net;

namespace DigiMixer.Diagnostics;

public record AnnotatedMessage<TMessage>(
    TMessage Message,
    DateTimeOffset Timestamp,
    MessageDirection Direction,
    int StreamOffset,
    IPAddress SourceAddress,
    IPAddress DestinationAddress) where TMessage : class, IMixerMessage<TMessage>
{    
}
