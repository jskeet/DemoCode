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
    private readonly UdpClient receivingClient;
    private readonly UdpClient sendingClient;
    private readonly CancellationTokenSource cts;

    public event EventHandler<OscPacket>? PacketReceived;

    /// <summary>
    /// Constructs a UDP client where the same port is used for both inbound and outbound communication.
    /// </summary>
    public UdpOscClient(ILogger logger, string host, int port)
    {
        this.logger = logger;
        sendingClient = new UdpClient();
        sendingClient.Connect(host, port);
        receivingClient = sendingClient;
        cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Constructs a UDP client which connects to <paramref name="host"/> on
    /// <paramref name="outboundPort"/>, but listens for messages on <paramref name="inboundPort"/>.
    /// </summary>
    public UdpOscClient(ILogger logger, string host, int outboundPort, int inboundPort)
    {
        this.logger = logger;
        sendingClient = new UdpClient();
        sendingClient.Connect(host, outboundPort);
        receivingClient = new UdpClient(inboundPort);
        cts = new CancellationTokenSource();
    }

    public Task SendAsync(OscPacket packet)
    {
        if (logger.IsEnabled(LogLevel.Trace) && packet is OscMessage message)
        {
            logger.LogTrace("Sending OSC message: {message}", message.ToLogFormat());
        }
        var data = packet.ToByteArray();
        return sendingClient.SendAsync(data, data.Length);
    }

    public async Task StartReceiving()
    {
        try
        {
            while (!cts.IsCancellationRequested)
            {
                var result = await receivingClient.ReceiveAsync(cts.Token);
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
        receivingClient?.Dispose();
        sendingClient?.Dispose();
        // TODO: dispose of receivingClient and sendingClient?
    }
}
