// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.Reflection.Metadata;
using System.Reflection;
using System.Threading.Tasks;

namespace OscMixerControl.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = args[0];
            var port = int.Parse(args[1]);
            var client = new UdpOscClient(host, port);

            client.PacketReceived += ReceivePacket;

            await SendMessage(new OscMessage("/xremote"));


            await Task.Delay(15000);

            async Task SendMessage(OscMessage message)
            {
                await client.SendAsync(message);
                System.Console.WriteLine("Sent packet " + message);
            }

            void ReceivePacket(object sender, OscPacket packet)
            {
                System.Console.WriteLine(packet);
            }
        }
    }
}
