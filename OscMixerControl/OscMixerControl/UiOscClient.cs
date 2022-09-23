// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OscMixerControl;

/// <summary>
/// Client for Ui protocol mixers (e.g. the Soundcraft Ui24R), translating OSC messages
/// from applications into roughly-corresponding Ui protocol requests, and UI protocol
/// responses into OSC messages.
/// </summary>
public sealed class UiOscClient : IOscClient
{
    private readonly TcpClient client;
    private readonly NetworkStream stream;
    private readonly CancellationTokenSource cts;
    public event EventHandler<OscPacket> PacketReceived;

    public UiOscClient(string hostName, int port)
    {
        client = new TcpClient();
        client.Connect(hostName, port);
        stream = client.GetStream();
        cts = new CancellationTokenSource();
        StartReceiving();
    }

    public void Dispose() => cts.Cancel();

    private async void StartReceiving()
    {
        var reader = new StreamReader(stream);
        while (!cts.IsCancellationRequested)
        {
            // TODO: Handle cancellation...
            string line = await reader.ReadLineAsync();
            foreach (var packet in TranslateUiToOsc(line))
            {
                // TODO: Maybe do this asynchronously...
                PacketReceived?.Invoke(this, packet);
            }
        }
    }

    private IEnumerable<OscPacket> TranslateUiToOsc(string line)
    {
        yield break;
    }

    public async Task SendAsync(OscPacket packet)
    {
        foreach (var line in TranslateOscToUi(packet))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(line + "\n");
            await stream.WriteAsync(bytes, 0, bytes.Length, cts.Token);
        }
    }

    private IEnumerable<string> TranslateOscToUi(OscPacket packet)
    {
        yield break;
    }
}
