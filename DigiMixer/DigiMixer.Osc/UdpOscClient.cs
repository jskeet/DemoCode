// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System.Net.Sockets;

namespace OscMixerControl;

internal sealed class UdpOscClient : IOscClient
{
    private readonly UdpClient client;
    private readonly CancellationTokenSource cts;

    public event EventHandler<OscPacket>? PacketReceived;

    public UdpOscClient(string host, int port)
    {
        client = new UdpClient();
        client.Connect(host, port);
        cts = new CancellationTokenSource();
    }

    public Task SendAsync(OscPacket packet)
    {
        var data = packet.ToByteArray();
        return client.SendAsync(data, data.Length);
    }

    public async Task StartReceiving()
    {
        while (!cts.IsCancellationRequested)
        {
            var result = await client.ReceiveAsync();
            var buffer = result.Buffer;
            var packet = OscPacket.Read(buffer, 0, buffer.Length);
            // TODO: Maybe do this asynchronously...
            PacketReceived?.Invoke(this, packet);
        }
    }

    public void Dispose() => cts.Cancel();
}
