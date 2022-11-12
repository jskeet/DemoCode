// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OscMixerControl
{
    public sealed class UdpOscClient : IOscClient
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

        public async Task SendAsync(OscPacket packet)
        {
            var data = packet.ToByteArray();
            await client.SendAsync(data, data.Length).ConfigureAwait(false);
        }

        public async Task SendAsync(OscPacket packet, IPEndPoint endPoint)
        {
            var data = packet.ToByteArray();
            await client.SendAsync(data, data.Length, endPoint).ConfigureAwait(false);
        }

        // TODO: Avoid async void methods, and observe exceptions more appropriately.
        private async void StartReceiving()
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var result = await client.ReceiveAsync();
                    var buffer = result.Buffer;
                    var packet = OscPacket.Read(buffer, 0, buffer.Length);
                    // TODO: Maybe do this asynchronously...
                    PacketReceived?.Invoke(this, packet);
                }
                catch (SocketException)
                {
                    // We'll keep trying to receive data; we expect
                    // the caller to dispose of this object (which will break
                    // the loop) if they want to stop fully.
                    await Task.Delay(1000);
                }
                catch (ObjectDisposedException)
                {
                    // Just a timing issue; we'll exit the loop imminently.
                }
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            try
            {
                client.Dispose();
            }
            catch
            {
                // If disposing the UdpClient fails, there's really nothing we can
                // we about it, and it shouldn't do any further harm. Just swallow the error.
            }
        }
    }
}
