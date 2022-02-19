// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CameraControl.Visca
{
    internal sealed class TcpViscaClient : ViscaClientBase
    {
        private readonly TcpSendLock sendLock;
        private readonly byte[] writeBuffer = new byte[16];
        private readonly ReadBuffer buffer = new();
        private TcpClient? client;
        private Stream? stream;
        private bool firstTime = true;

        public string Host { get; }
        public int Port { get; }

        public override void Dispose() => client?.Dispose();

        public TcpViscaClient(string host, int port, ILogger? logger, TcpSendLock? sendLock) : base(logger) =>
            (Host, Port, this.sendLock) = (host, port, sendLock ?? new TcpSendLock(null));


        protected override async Task ReconnectAsync(CancellationToken cancellationToken)
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
            buffer.Clear();
            client?.Dispose();
            client = new TcpClient { NoDelay = true };
#if NET5_0_OR_GREATER
            try
            {
                await client.ConnectAsync(Host, Port, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                client = null;
                throw;
            }
#else
            // This is really ugly, but it's the only way we can cancel a connection attempt
            // in netstandard2.0.
            using (cancellationToken.Register(() => client.Dispose()))
            {
                try
                {
                    await client.ConnectAsync(Host, Port).ConfigureAwait(false);
                }
                catch
                {
                    client = null;
                    // If we're failing because the cancellation token was cancelled,
                    // that takes priority.
                    cancellationToken.ThrowIfCancellationRequested();
                    // Otherwise, let the normal exception propagate.
                    throw;
                }
            }
#endif
            stream = client.GetStream();
        }

        protected override async Task SendPacketAsync(ViscaPacket packet, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                await ReconnectAsync(cancellationToken).ConfigureAwait(false);
            }
            for (int i = 0; i < packet.Length; i++)
            {
                writeBuffer[i] = packet.GetByte(i);
            }
            await sendLock.AcquireAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // stream is never null if either client wasn't null, or after reconnection.
#if NET5_0_OR_GREATER
                await stream!.WriteAsync(writeBuffer.AsMemory(0, packet.Length), cancellationToken).ConfigureAwait(false);
#else
                await stream!.WriteAsync(writeBuffer, 0, packet.Length, cancellationToken).ConfigureAwait(false);
#endif
                await sendLock.PostSendDelayAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                sendLock.Release();
            }
        }

        protected override Task<ViscaPacket> ReceivePacketAsync(CancellationToken cancellationToken)
        {
            if (stream is null)
            {
                throw new ViscaProtocolException("Cannot receive a packet before sending one");
            }
            return buffer.ReadAsync(stream, cancellationToken);
        }
    }
}
