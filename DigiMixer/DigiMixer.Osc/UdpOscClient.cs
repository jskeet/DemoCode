// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using OscCore;
using System.Diagnostics;
using System.Net.Sockets;

namespace OscMixerControl;

internal sealed class UdpOscClient : IOscClient
{
    private readonly ILogger logger;
    private readonly UdpClient client;
    private readonly CancellationTokenSource cts;

    public event EventHandler<OscPacket>? PacketReceived;

    public UdpOscClient(ILogger logger, string host, int port)
    {
        this.logger = logger;
        client = new UdpClient();
        client.Connect(host, port);
        cts = new CancellationTokenSource();
    }

    public Task SendAsync(OscPacket packet)
    {
        if (logger.IsEnabled(LogLevel.Trace) && packet is OscMessage message)
        {
            logger.LogTrace("Sending OSC message: {address} {message}", message.Address, message);
        }
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
            if (logger.IsEnabled(LogLevel.Trace) && packet is OscMessage message)
            {
                logger.LogTrace("Received OSC message: {address} {message}", message.Address, message);
            }
            // TODO: Maybe do this asynchronously...
            PacketReceived?.Invoke(this, packet);
        }
    }

    public void Dispose() => cts.Cancel();
}
