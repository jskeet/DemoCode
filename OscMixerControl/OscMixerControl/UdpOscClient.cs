// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OscMixerControl
{
    public sealed class UdpOscClient : IDisposable
    {
        private readonly UdpClient client;
        private readonly CancellationTokenSource cts;

        public event EventHandler<OscPacket> PacketReceived;

        public UdpOscClient(string hostName, int port)
        {
            client = new UdpClient();
            client.Connect(hostName, port);
            cts = new CancellationTokenSource();
            StartReceiving();
        }

        public Task SendAsync(OscPacket packet)
        {
            var data = packet.ToByteArray();
            return client.SendAsync(data, data.Length);
        }

        private async void StartReceiving()
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
}
