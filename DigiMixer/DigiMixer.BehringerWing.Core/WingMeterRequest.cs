using System.Buffers.Binary;
using System.Collections.Immutable;
using static DigiMixer.BehringerWing.Core.WingMeterRequest;

namespace DigiMixer.BehringerWing.Core;

public partial record WingMeterRequest(ushort? UdpPort, uint ReportId, ImmutableList<ChannelList> ChannelLists)
{
    public int GetFirstDataOffset(WingMeterType type)
    {
        int offset = 0;
        foreach (var list in ChannelLists)
        {
            if (list.Type == type)
            {
                return offset;
            }
            // TODO: Handle channel types with different sizes. (At the moment we only ever need v2 data.)
            offset += list.Ids.Count * WingMeterMessage.ChannelV2Data.Size;
        }
        throw new ArgumentException($"Channel type {type} is not in the request");
    }

    internal int CopyTo(Span<byte> span)
    {
        int length = 0;
        if (UdpPort is ushort port)
        {
            span[length++] = 0xd3;
            BinaryPrimitives.WriteUInt16BigEndian(span[length..], port);
            length += 2;
        }
        span[length++] = 0xd4;
        BinaryPrimitives.WriteUInt32BigEndian(span[length..], ReportId);
        length += 4;
        span[length++] = 0xdc;
        foreach (var list in ChannelLists)
        {
            length += list.CopyTo(span[length..]);
        }
        span[length++] = 0xde; // End of meter collection
        return length;
    }

    /// <summary>
    /// List of channels of a particular type to request data for.
    /// Note that these are 1-based even though the serialized form is 0-based.
    /// </summary>
    public record ChannelList(WingMeterType Type, ImmutableList<int> Ids)
    {
        internal int CopyTo(Span<byte> span)
        {
            span[0] = (byte) Type;
            for (int i = 0; i < Ids.Count; i++)
            {
                span[i + 1] = (byte) (Ids[i] - 1);
            }
            return 1 + Ids.Count;
        }
    }
}
