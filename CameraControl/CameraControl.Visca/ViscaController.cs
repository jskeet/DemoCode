// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CameraControl.Visca
{
    public class ViscaController : IDisposable
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        public const byte MinSpeed = 0x01;
        public const byte MaxPanTiltSpeed = 0x18;
        public const byte MaxZoomSpeed = 0x7;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        
        public TimeSpan CommandTimeout { get; }
        public string Host { get; }
        public int Port { get; }
        private readonly ReadBuffer buffer = new ReadBuffer();
        private TcpClient client;
        private NetworkStream stream;

        public ViscaController(string host, int port, TimeSpan commandTimeout)
        {
            (Host, Port, CommandTimeout) = (host, port, commandTimeout);
        }

        public ViscaController(string host, int port) : this(host, port, DefaultTimeout)
        {
        }

        private async Task Reconnect(CancellationToken cancellationToken)
        {
            buffer.Clear();
            client?.Dispose();
            client = new TcpClient { NoDelay = true };

            // This is really ugly, but it's the only way we can cancel a connection attempt.
            using (cancellationToken.Register(() => client.Dispose()))
            {
                try
                {
                    await client.ConnectAsync(Host, Port);
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
            stream = client.GetStream();
        }

        public async Task PowerOn(CancellationToken cancellationToken = default)
        {
            await SendAsync(cancellationToken, 0x81, 0x01, 0x04, 0x00, 0x02, 0xff);
            await Task.Delay(1000);
            // TODO: Make this more reliable...
            int attemptsLeft = 30;
            while (true)
            {
                try
                {
                    await GetPowerStatus();
                    return;
                }
                catch (Exception)
                {
                    await Task.Delay(1000);
                    attemptsLeft--;
                    if (attemptsLeft == 0)
                    {
                        throw;
                    }
                }
            }
        }

        public async Task PowerOff(CancellationToken cancellationToken = default) =>
            await SendAsync(cancellationToken, 0x81, 0x01, 0x04, 0x00, 0x03, 0xff);

        public void Dispose() => client.Dispose();

        public async Task<PowerStatus> GetPowerStatus(CancellationToken cancellationToken = default)
        {
            var bytes = await SendAsync(cancellationToken, 0x81, 0x09, 0x04, 0x00, 0xff);
            return (PowerStatus) bytes[2];
        }

        public async Task<short> GetZoom(CancellationToken cancellationToken = default)
        {
            var bytes = await SendAsync(cancellationToken, 0x81, 0x09, 0x04, 0x47, 0xff);
            return GetInt16(bytes, 2);
        }

        public async Task<(short pan, short tilt)> GetPanTilt(CancellationToken cancellationToken = default)
        {
            var bytes = await SendAsync(cancellationToken, 0x81, 0x09, 0x06, 0x12, 0xff);
            return (GetInt16(bytes, 2), GetInt16(bytes, 6));
        }

        public async Task SetZoom(short zoom, CancellationToken cancellationToken = default)
        {
            byte[] bytes = new byte[] { 0x81, 0x01, 0x04, 0x47, 0, 0, 0, 0, 0xff };
            SetInt16(bytes, 4, zoom);
            await SendAsync(cancellationToken, bytes);
        }

        public async Task ContinuousPanTilt(bool? pan, bool? tilt, byte panSpeed, byte tiltSpeed, CancellationToken cancellationToken = default)
        {
            byte actualPan = (byte) (pan is null ? 3 : pan.Value ? 2 : 1);
            byte actualTilt = (byte) (tilt is null ? 3 : tilt.Value ? 2 : 1);
            byte[] bytes = new byte[] { 0x81, 0x01, 0x06, 0x1, panSpeed, tiltSpeed, actualPan, actualTilt, 0xff };
            await SendAsync(cancellationToken, bytes);
        }

        public async Task ContinuousZoom(bool? zoomIn, byte zoomSpeed, CancellationToken cancellationToken = default)
        {
            byte parameter = (byte) (zoomIn is null ? 0 : zoomIn.Value ? 0x20 | zoomSpeed : 0x30 | zoomSpeed);
            await SendAsync(cancellationToken, 0x81, 0x01, 0x04, 0x07, parameter, 0xff);
        }

        public async Task SetPanTilt(short pan, short tilt, byte panSpeed, byte tiltSpeed, CancellationToken cancellationToken = default)
        {
            byte[] bytes = new byte[] { 0x81, 0x01, 0x06, 0x02, panSpeed, tiltSpeed, 0, 0, 0, 0, 0, 0, 0, 0, 0xff };
            SetInt16(bytes, 6, pan);
            SetInt16(bytes, 10, tilt);
            await SendAsync(cancellationToken, bytes);
        }

        private void SetInt16(byte[] buffer, int start, short value)
        {
            buffer[start] = (byte) ((value >> 12) & 0xf);
            buffer[start + 1] = (byte) ((value >> 8) & 0xf);
            buffer[start + 2] = (byte) ((value >> 4) & 0xf);
            buffer[start + 3] = (byte) ((value >> 0) & 0xf);
        }

        private short GetInt16(byte[] buffer, int start) =>
            (short) (
            (buffer[start] << 12) |
            (buffer[start + 1] << 8) |
            (buffer[start + 2] << 4) |
            (buffer[start + 3]) << 0);

        public async Task GoHome(CancellationToken cancellationToken = default) =>
            await SendAsync(cancellationToken, 0x81, 0x01, 0x06, 0x04, 0xff);

        private async Task<byte[]> SendAsync(CancellationToken cancellationToken, params byte[] packet)
        {
            var effectiveToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, new CancellationTokenSource(CommandTimeout).Token).Token;
            bool reconnect = true;
            await semaphore.WaitAsync(effectiveToken);
            try
            {
                if (client is null)
                {
                    await Reconnect(cancellationToken);
                }
                await stream.WriteAsync(packet, 0, packet.Length, effectiveToken);
                while (true)
                {
                    byte[] returnPacket = await buffer.ReadAsync(stream, effectiveToken);
                    if (returnPacket.Length == 1)
                    {
                        throw new ViscaProtocolException($"Received single byte from VISCA interface");
                    }
                    switch (returnPacket[1] >> 4)
                    {
                        // Command received
                        case 4:
                            continue;
                        // Success
                        case 5:
                            reconnect = false;
                            return returnPacket;
                        // Error reported by device
                        case 6:
                            // TODO: Create an appropriate exception type for this.
                            throw new Exception($"Error reported by VISCA interface. Error data: {BitConverter.ToString(returnPacket)}");
                        // VISCA protocol violation
                        default:
                            throw new ViscaProtocolException($"Invalid packet returned from VISCA interface. Error data: {BitConverter.ToString(returnPacket)}");
                    }
                }
            }
            finally
            {
                if (reconnect)
                {
                    await Reconnect(cancellationToken);
                }
                semaphore.Release();
            }
        }
    }
}
