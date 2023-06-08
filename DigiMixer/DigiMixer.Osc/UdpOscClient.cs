// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using OscCore;

namespace DigiMixer.Osc;

internal sealed class UdpOscClient : UdpControllerBase, IOscClient
{
    public event EventHandler<OscPacket>? PacketReceived;

    /// <summary>
    /// Constructs a UDP client which connects to <paramref name="host"/> on
    /// <paramref name="remotePort"/>, with an optionally specified local port.
    /// </summary>
    public UdpOscClient(ILogger logger, string host, int remotePort, int? localPort = null) : base(logger, host, remotePort, localPort)
    {
    }

    public Task SendAsync(OscPacket packet)
    {
        if (Logger.IsEnabled(LogLevel.Trace) && packet is OscMessage message)
        {
            Logger.LogTrace("Sending OSC message: {message}", message.ToLogFormat());
        }
        var data = packet.ToByteArray();
        // TODO: Cancellation token parameter?
        return Send(data, default);
    }

    protected override void ProcessData(ReadOnlySpan<byte> data) =>
        throw new NotSupportedException("Can't parse an OscPacket via a span");

    protected override void ProcessData(byte[] data, int length)
    {
        var packet = OscPacket.Read(data, 0, data.Length);
        if (Logger.IsEnabled(LogLevel.Trace) && packet is OscMessage message)
        {
            Logger.LogTrace("Received OSC message: {message}", message.ToLogFormat());
        }
        // TODO: Maybe do this asynchronously...
        PacketReceived?.Invoke(this, packet);
    }
}
