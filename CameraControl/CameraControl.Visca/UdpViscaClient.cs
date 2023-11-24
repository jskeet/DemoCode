// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace CameraControl.Visca;

internal sealed class UdpViscaClient : ViscaClientBase
{
    private readonly byte[] readBuffer = new byte[32];
    private readonly byte[] writeBuffer = new byte[32];
    private UdpClient? client;
    private bool firstTime;

    public string Host { get; }
    public int Port { get; }

    public UdpViscaClient(string host, int port, ViscaMessageFormat format, ILogger? logger) : base(format, logger) =>
        (Host, Port) = (host, port);

    public override void Dispose()
    {
        client?.Dispose();
        client = null;
    }

    protected override void Disconnect() => Dispose();

    protected async override Task<ViscaMessage> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        int bytesRead = await client!.Client.ReceiveAsync(readBuffer, SocketFlags.None, cancellationToken);
        if (ViscaMessage.Parse(readBuffer.AsSpan().Slice(0, bytesRead), Format) is not ViscaMessage message)
        {
            throw new ViscaProtocolException("UDP packet did not include end of VISCA packet");
        }
        if (message.Length != bytesRead)
        {
            throw new ViscaProtocolException("UDP packet contained more than one message");
        }
        return message;
    }

    private void Reconnect()
    {
        if (firstTime)
        {
            Logger?.LogDebug("Connecting to {host}:{port}", Host, Port);
            firstTime = false;
        }
        else
        {
            Logger?.LogDebug("Reconnecting to {host}:{port}", Host, Port);
        }
        client?.Dispose();
        client = new UdpClient();
        client.Connect(Host, Port);
    }

    protected async override Task SendMessageAsync(ViscaMessage message, CancellationToken cancellationToken)
    {
        if (client is null)
        {
            Reconnect();
        }
        message.WriteTo(writeBuffer);
        
        await client!.SendAsync(new ReadOnlyMemory<byte>(writeBuffer, 0, message.Length), cancellationToken);
    }
}
