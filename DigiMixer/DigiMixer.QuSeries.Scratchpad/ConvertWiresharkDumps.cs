// See https://aka.ms/new-console-template for more information
using DigiMixer.Diagnostics;
using DigiMixer.QuSeries.Core;
using System.Net;
using System.Net.Sockets;
using System.Text;

class ConvertWiresharkDumps
{
    static void Main()
    {
        var directory = @"C:\Users\skeet\OneDrive\Documents\Qu-SB dumps";
        foreach (var file in Directory.GetFiles(directory, "*.pcapng"))
        {
            ConvertFile(file);
        }
    }

    static void ConvertFile(string file)
    {
        var dump = WiresharkDump.Load(file);
        var packets = dump.IPV4Packets.ToList();
        var buffer = new QuPacketBuffer();
        var clientAddr1 = IPAddress.Parse("192.168.1.140");
        var clientAddr2 = IPAddress.Parse("192.168.1.230");
        var mixerAddr = IPAddress.Parse("192.168.1.60");
        foreach (var packet in packets)
        {
            if (packet.Type == ProtocolType.Tcp && packet.Source.Address.Equals(mixerAddr) &&
                (packet.Dest.Address.Equals(clientAddr1) || packet.Dest.Address.Equals(clientAddr2)))
            {
                buffer.Process(packet.Data, MaybeSaveConverted);
            }
        }

        void MaybeSaveConverted(QuControlPacket packet)
        {
            if (packet is not QuGeneralPacket qgp)
            {
                return;
            }

            var data = qgp.Data;
            if (data.Length != 25888)
            {
                return;
            }
            string outputFile = Path.ChangeExtension(file, "pcapng-decoded");
            using var writer = File.CreateText(outputFile);
            int address = 0;
            while (address + 16 < data.Length)
            {
                int lineLength = Math.Min(data.Length - address, 16);
                var line = new StringBuilder();
                line.Append(address.ToString("X4"));
                line.Append("  ");
                AppendHex(line, data.Slice(address, Math.Min(lineLength, 8)));
                if (lineLength > 8)
                {
                    line.Append("  ");
                    AppendHex(line, data.Slice(address + 8, Math.Min(lineLength - 8, 8)));
                }
                while (line.Length < 56)
                {
                    line.Append(' ');
                }
                for (int i = 0; i < lineLength; i++)
                {
                    if (i == 8)
                    {
                        line.Append("  ");
                    }
                    byte b = data[address + i];
                    line.Append(b >= ' ' && b <= '~' ? (char) b : '.');
                }
                writer.WriteLine(line.ToString());
                address += 16;
            }
            Console.WriteLine($"Created {outputFile}");

            void AppendHex(StringBuilder builder, ReadOnlySpan<byte> bytes)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (i != 0)
                    {
                        builder.Append(' ');
                    }
                    builder.Append(bytes[i].ToString("x2"));
                }
            }
        }
    }
}