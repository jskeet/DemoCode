using DigiMixer.Mackie.Core;
using Google.Protobuf;

namespace DigiMixer.MackieDump;

public partial class Packet
{
    public static Packet FromMackiePacket(MackiePacket packet, bool outbound, DateTimeOffset? timestamp) =>
        new Packet
        {
            Outbound = outbound,
            Command = (int) packet.Command,
            Type = (int) packet.Type,
            Data = ByteString.CopyFrom(packet.Body.Data),
            Sequence = packet.Sequence,
            Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(timestamp ?? DateTimeOffset.UtcNow)
        };
}
