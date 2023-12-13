﻿using DigiMixer.Diagnostics;
using DigiMixer.DmSeries.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiMixer.DmSeries.Tools;

public class StartClient : Tool
{
    private const string Host = "192.168.1.86";
    private const int Port = 50368;

    public override async Task<int> Execute()
    {
        var client = new DmClient(NullLogger.Instance, Host, Port);
        client.MessageReceived += (sender, message) => Console.WriteLine(message);
        await client.Connect(default);
        client.Start();

        var message1 = new DmMproMessage(new DmMproMessage.Chunk([0x01, 0x01, 0x01, 0x02, 0x31, 0x00, 0x00, 0x00, 0x09, 0x50, 0x72, 0x6f, 0x70, 0x65, 0x72, 0x74, 0x79, 0x00, 0x11, 0x00, 0x00, 0x00, 0x01, 0x80]));
        byte[] message2Data = [0x11, 0x00, 0x00, 0x00, 0x42, 0x01, 0x10, 0x01,
            0x04, 0x11, 0x00, 0x00, 0x00, 0x01, 0x00, 0x31, 0x00, 0x00, 0x00, 0x09, 0x50, 0x72, 0x6f, 0x70,
            0x65, 0x72, 0x74, 0x79, 0x00, 0x11, 0x00, 0x00, 0x00, 0x10, 0x3a, 0x7c, 0x8d, 0x4c, 0x85, 0xf8,
            0x9f, 0x1e, 0xaa, 0x83, 0x4f, 0x96, 0x63, 0x0c, 0xec, 0x3d, 0x11, 0x00, 0x00, 0x00, 0x10, 0x20,
            0xdc, 0xd8, 0x75, 0x45, 0x50, 0xdd, 0x5c, 0xb1, 0x0d, 0x89, 0x00, 0xae, 0x71, 0x4a, 0x04];
        var message2 = new DmRawMessage("MPRO", message2Data);
        var message3 = new DmMproMessage(new DmMproMessage.Chunk([0x01, 0x04, 0x01, 0x00]));

        await client.SendAsync(message1.RawMessage, default);
        await client.SendAsync(message2, default);
        await client.SendAsync(message3.RawMessage, default);

        while (true)
        {
            await Task.Delay(1000);
        }
    }
}