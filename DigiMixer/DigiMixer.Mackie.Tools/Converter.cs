using DigiMixer.Core;
using DigiMixer.Diagnostics;
using DigiMixer.Mackie.Core;
using Google.Protobuf;
using System.Net;
using System.Net.Sockets;

namespace DigiMixer.Mackie.Tools;

/// <summary>
/// Converts from pcapng to message proto
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
        var mc = new MessageCollection();

        var outboundProcessor = new MessageProcessor<MackieMessage>(
            MackieMessage.TryParse,
            message => message.Length,
            message => ConvertAndStoreMessage(message, true),
            65540);
        var inboundProcessor = new MessageProcessor<MackieMessage>(
            MackieMessage.TryParse,
            message => message.Length,
            message => ConvertAndStoreMessage(message, false),
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

        Console.WriteLine($"Captured {mc.Messages.Count} messages");

        using var output = File.Create(outputFile);
        mc.WriteTo(output);

        void ConvertAndStoreMessage(MackieMessage mackieMessage, bool outbound)
        {
            var message = Message.FromMackieMessage(mackieMessage, outbound, new DateTimeOffset(currentTimestamp, TimeSpan.Zero));
            mc.Messages.Add(message);
        }
    }
}
