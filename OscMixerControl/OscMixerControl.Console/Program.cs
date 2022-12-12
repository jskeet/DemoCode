// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using OscMixerControl;
using System;
using System.Threading.Tasks;

var host = args[0];
var port = int.Parse(args[1]);
var client = new UdpOscClient(host, port);

client.PacketReceived += ReceivePacket;



while (true)
{
    await SendMessage(new OscMessage("/batchsubscribe", "/meters/2", "/meters/2", 0, 0, 20));
    await Task.Delay(6000);
}

async Task SendMessage(OscMessage message)
{
    await client.SendAsync(message);
    Log("Sent packet " + message);
}

void ReceivePacket(object sender, OscPacket packet)
{
    if (packet is OscMessage message && message.Count > 0 && message[0] is byte[] bytes)
    {
        Log($"10 => {GetValue(10):0.000000}");
        //Log($"25 => {GetValue(25):0.0000}");

        double GetValue(int index) =>
            BitConverter.ToSingle(bytes, index * 4 + 4);
    }
    else
    {
        Log(packet);
    }
}

void Log(object value) =>
    Console.WriteLine(value);
