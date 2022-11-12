using DigiMixer.Mackie;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiMixer.MackieDump;

internal class PacketBuffer
{
    private readonly string name;
    private byte[] buffer = new byte[65536];
    private int position = 0;

    internal PacketBuffer(string name)
    {
        this.name = name;
    }

    internal void Consume(byte[] data)
    {
        Buffer.BlockCopy(data, 0, buffer, position, data.Length);
        position += data.Length;

        while (position != 0 && MackiePacket.TryParse(buffer, 0, position) is MackiePacket packet)
        {
            Console.WriteLine($"{name}: {packet}");
            if (position == packet.Length)
            {
                position = 0;
            }
            else
            {
                Buffer.BlockCopy(buffer, packet.Length, buffer, 0, position - packet.Length);
                position -= packet.Length;
            }
        }
    }
}
