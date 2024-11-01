﻿using PcapngFile;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace DigiMixer.Diagnostics;

public class IPV4Packet
{
    private readonly byte[] data;
    private readonly int dataOffset;
    private readonly int dataLength;
    public ProtocolType Type { get; }
    public ReadOnlyMemory<byte> Data => data.AsMemory().Slice(dataOffset, dataLength);
    public IPEndPoint Source { get; }
    public IPEndPoint Dest { get; }
    public DateTime Timestamp { get; }

    private IPV4Packet(ProtocolType type, IPEndPoint source, IPEndPoint dest, byte[] data, int offset, int length, DateTime timestamp)
    {
        this.Type = type;
        this.Source = source;
        this.Dest = dest;
        this.data = data;
        this.dataOffset = offset;
        this.dataLength = length;
        Timestamp = timestamp;
    }

    public static IPV4Packet? TryConvert(BlockBase block)
    {
        if (block is not EnhancedPacketBlock packet)
        {
            return null;
        }
        var data = packet.Data;
        var dataSpan = data.AsSpan();
        if (data.Length < 40)
        {
            return null;
        }
        // IPv4
        if (data[12] != 0x8 || data[13] != 0x0)
        {
            return null;
        }
        var ipLength = BinaryPrimitives.ReadUInt16BigEndian(dataSpan.Slice(16));
        var type = (ProtocolType) data[23];
        IPAddress sourceAddress = new IPAddress(dataSpan.Slice(26, 4));
        IPAddress destAddress = new IPAddress(dataSpan.Slice(30, 4));
        int sourcePort = BinaryPrimitives.ReadUInt16BigEndian(dataSpan.Slice(34));
        int destPort = BinaryPrimitives.ReadUInt16BigEndian(dataSpan.Slice(36));

        int dataOffset;
        int dataLength;
        if (type == ProtocolType.Udp)
        {
            dataOffset = 42;
            dataLength = BinaryPrimitives.ReadUInt16BigEndian(dataSpan[38..]) - 8; // The header includes its own length
        }
        else if (type == ProtocolType.Tcp)
        {
            int headerLength = (data[46] & 0xf0) >> 2;
            dataOffset = 34 + headerLength;
            dataLength = ipLength - (dataOffset - 14);
        }
        else
        {
            return null;
        }
        return new IPV4Packet(type,
            new IPEndPoint(sourceAddress, sourcePort),
            new IPEndPoint(destAddress, destPort),
            data, dataOffset, dataLength, packet.GetTimestamp());
    }

    public override string ToString() => $"{Type}: {Source} => {Dest}: {dataLength} bytes";
}
