// See https://aka.ms/new-console-template for more information
using DigiMixer.Diagnostics;
using DigiMixer.QuSeries.Core;
using System.Net;
using System.Net.Sockets;

class DecodeWireshark
{
    static void Main()
    {
        var file = @"c:\users\skeet\Downloads\Wireshark\test1.pcapng";

        var dump = WiresharkDump.Load(file);
        var packets = dump.IPV4Packets.ToList();
        Console.WriteLine($"Packets: {packets.Count}");

        var clientAddr = IPAddress.Parse("192.168.1.221");
        var mixerAddr = IPAddress.Parse("192.168.1.60");

        var controlPackets = packets.Where(ClientMixerPacket).Where(packet => packet.Type == ProtocolType.Tcp).ToList();

        /*
        foreach (var cp in controlPackets)
        {
            Console.WriteLine(cp);
        }*/

        Console.WriteLine($"Control packets: {controlPackets.Count}");

        QuPacketBuffer clientBuffer = new QuPacketBuffer(65540);
        QuPacketBuffer mixerBuffer = new QuPacketBuffer(65540);

        foreach (var packet in controlPackets)
        {
            (var description, var buffer) = packet.Dest.Address.Equals(clientAddr)
                ? ("Mixer->Client", clientBuffer)
                : ("Client->Mixer", mixerBuffer);
            buffer.Process(packet.Data, quPacket => LogPacket(description, quPacket));
        }

        void LogPacket(string description, QuPacket packet) => Console.Write(packet);

        bool ClientMixerPacket(IPV4Packet packet) =>
            (packet.Source.Address.Equals(clientAddr) || packet.Source.Address.Equals(mixerAddr)) &&
            (packet.Dest.Address.Equals(clientAddr) || packet.Dest.Address.Equals(mixerAddr));
    }
}