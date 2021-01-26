// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
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

            client.PacketReceived += (sender, packet) => System.Console.WriteLine(packet);

            var message = new OscMessage("/info");

            await client.SendAsync(message);
            System.Console.WriteLine("Sent packet " + message);

            message = new OscMessage("/ch/04/mix/fader", 0.5f);
            await client.SendAsync(message);
            System.Console.WriteLine("Sent packet");

            /*
            message = new OscMessage("/xremote");
            await client.SendAsync(message);
            System.Console.WriteLine("Sent packet");
            */
            /*
            message = new OscMessage("/subscribe", "/ch/01/mix/fader", 10);
            await client.SendAsync(message);
            System.Console.WriteLine("Sent packet");
            */

            message = new OscMessage("/showdump");
            await client.SendAsync(message);
            System.Console.WriteLine("Sent packet");

            await Task.Delay(15000);
        }
    }
}
