using DigiMixer.Core;
using DigiMixer.Diagnostics;
using DigiMixer.Mackie.Core;
using Google.Protobuf;
using System.Net.Sockets;
using System.Net;

namespace DigiMixer.MackieDump;

/// <summary>
/// Converts from pcapng to packet proto
/// </summary>
internal class Converter
{
    internal static void Execute(string inputFile, string clientAddress, string mixerAddress, string outputFile)
    {
        IPAddress clientAddr = IPAddress.Parse(clientAddress);
        IPAddress mixerAddr = IPAddress.Parse(mixerAddress);
        var dump = WiresharkDump.Load(inputFile);
        var packets = dump.IPV4Packets.ToList();

        // This is updated before Process is called; it's the timestamp of the latest packet.
        DateTime currentTimestamp = new DateTime(2000, 1, 1, 0, 0, 0);
        var pc = new PacketCollection();

        var outboundProcessor = new MessageProcessor<MackiePacket>(
            MackiePacket.TryParse,
            packet => packet.Length,
            packet => ConvertAndStorePacket(packet, true),
            65540);
        var inboundProcessor = new MessageProcessor<MackiePacket>(
            MackiePacket.TryParse,
            packet => packet.Length,
            packet => ConvertAndStorePacket(packet, false),
            65540);
        foreach (var packet in packets)
        {
            if (packet.Type != ProtocolType.Tcp)
            {
                continue;
            }
            currentTimestamp = packet.Timestamp;
            if (packet.Source.Address.Equals(clientAddr) && packet.Dest.Address.Equals(mixerAddr))
            {
                outboundProcessor.Process(packet.Data);
            }
            else if (packet.Source.Address.Equals(mixerAddr) && packet.Dest.Address.Equals(clientAddr))
            {
                inboundProcessor.Process(packet.Data);
            }
        }

        Console.WriteLine($"Captured {pc.Packets.Count} packets");

        using var output = File.Create(outputFile);
        pc.WriteTo(output);

        void ConvertAndStorePacket(MackiePacket mackiePacket, bool outbound)
        {
            var packet = Packet.FromMackiePacket(mackiePacket, outbound, new DateTimeOffset(currentTimestamp, TimeSpan.Zero));
            pc.Packets.Add(packet);
        }
    }
}
