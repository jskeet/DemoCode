using DigiMixer.Mackie.Core;
using Google.Protobuf;

namespace DigiMixer.Mackie.Tools;

public partial class Message
{
    public static Message FromMackieMessage(MackieMessage message, bool outbound, DateTimeOffset? timestamp) =>
        new Message
        {
            Outbound = outbound,
            Command = (int) message.Command,
            Type = (int) message.Type,
            Data = ByteString.CopyFrom(message.Body.Data),
            Sequence = message.Sequence,
            Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(timestamp ?? DateTimeOffset.UtcNow)
        };
}
