// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using OscMixerControl;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

if (args.Length != 4)
{
    Console.WriteLine("Arguments: <listening address> <listening port> <mixer address> <mixer port>");
    return 1;
}

string listeningAddress = args[0];
int listeningPort = int.Parse(args[1]);
string mixerAddress = args[2];
int mixerPort = int.Parse(args[3]);

var server = new UdpClient(new IPEndPoint(IPAddress.Parse(listeningAddress), listeningPort));
var mixerClient = new UdpOscClient(mixerAddress, mixerPort);
int mixerPackets = 0;
int uiPackets = 0;
ConcurrentDictionary<string, int> uiPacketsByAddress = new ConcurrentDictionary<string, int>();
StartReceiving();

IPEndPoint clientEndpoint = null;

mixerClient.PacketReceived += async (sender, packet) =>
{
    mixerPackets++;
    if (packet is OscMessage message && !message.Address.StartsWith("meters") && !message.Address.StartsWith("/bus"))
    {
        var array = message.ToArray();
        for (int i = 0; i < array.Length; i++)
        {


            if (array[i] is string p && p == mixerAddress)
            {
                Console.WriteLine($"Received {message.Address} from target with parameters {string.Join(", ", message)}");
                array[i] = listeningAddress;
                Console.WriteLine($"Replaced parameter {i}");
            }
        }
    }
    if (clientEndpoint is not null)
    {
        var data = packet.ToByteArray();
        await server.SendAsync(data, data.Length, clientEndpoint);
    }
};

while (true)
{
    Console.WriteLine("Listening...");
    Thread.Sleep(TimeSpan.FromMinutes(1));
    Console.WriteLine($"Packets from mixer: {mixerPackets}; packets from UI: {uiPackets}");
    foreach (var pair in uiPacketsByAddress)
    {
        Console.WriteLine($"{pair.Key}: {pair.Value}");
    }
}

async void StartReceiving()
{
    while (true)
    {
        var result = await server.ReceiveAsync();
        clientEndpoint = result.RemoteEndPoint;
        var buffer = result.Buffer;
        var packet = OscPacket.Read(buffer, 0, buffer.Length);
        uiPackets++;
        if (packet is OscMessage message)
        {
            string[] parts = message.Address.Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                bool allDigits = true;
                for (int j = 0; j < part.Length && allDigits; j++)
                {
                    if (!char.IsDigit(part[j]))
                    {
                        allDigits = false;
                    }
                }
                if (allDigits && part.Length > 0)
                {
                    parts[i] = "xxx";
                }
            }
            string address = string.Join('/', parts);
            uiPacketsByAddress.AddOrUpdate(address, 1, (key, value) => value + 1);
        }
        /*
        if (packet is OscMessage message)
        {
            Console.WriteLine($"Received {message.Address} from client with parameters {string.Join(", ", message)}");
        }*/
        await mixerClient.SendAsync(packet);
    }
}
