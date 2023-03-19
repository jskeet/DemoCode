// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using OscCore;
using System.Net.Sockets;

namespace DigiMixer.Osc;

internal sealed class UdpOscClient : IOscClient
{
    private readonly ILogger logger;
    private readonly UdpClient client;
    private readonly CancellationTokenSource cts;

    public event EventHandler<OscPacket>? PacketReceived;

    /// <summary>
    /// Constructs a UDP client which connects to <paramref name="host"/> on
    /// <paramref name="remotePort"/>, with an optionally specified local port.
    /// </summary>
    public UdpOscClient(ILogger logger, string host, int remotePort, int? localPort = null)
    {
        this.logger = logger;
        client = localPort is null ? new UdpClient() : new UdpClient(localPort.Value);
        client.Connect(host, remotePort);
        cts = new CancellationTokenSource();
    }

    public Task SendAsync(OscPacket packet)
    {
        if (logger.IsEnabled(LogLevel.Trace) && packet is OscMessage message)
        {
            logger.LogTrace("Sending OSC message: {message}", message.ToLogFormat());
        }
        var data = packet.ToByteArray();
        return client.SendAsync(data, data.Length);
    }

    public async Task StartReceiving()
    {
        try
        {
            while (!cts.IsCancellationRequested)
            {
                // TODO: Use a single buffer repeatedly.
                var result = await client.ReceiveAsync(cts.Token);
                var buffer = result.Buffer;
                var packet = OscPacket.Read(buffer, 0, buffer.Length);
                if (logger.IsEnabled(LogLevel.Trace) && packet is OscMessage message)
                {
                    logger.LogTrace("Received OSC message: {message}", message.ToLogFormat());
                }
                // TODO: Maybe do this asynchronously...
                PacketReceived?.Invoke(this, packet);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in receive loop");
            throw;
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        client.Dispose();
    }
}
