// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace CameraControl.Visca
{
    public sealed class ViscaController : IDisposable
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        public const byte MinSpeed = 0x01;
        public const byte MaxPanTiltSpeed = 0x18;
        public const byte MaxZoomSpeed = 0x7;

        private TimeSpan CommandTimeout { get; }
        private readonly IViscaClient client;

        internal ViscaController(IViscaClient client, TimeSpan commandTimeout)
        {
            this.client = client;
            this.CommandTimeout = commandTimeout;
        }

        public static ViscaController ForTcp(string host, int port, TimeSpan? commandTimeout = null)
        {
            var client = new TcpViscaClient(host, port);
            return new ViscaController(client, commandTimeout ?? DefaultTimeout);
        }

        public async Task PowerOn(CancellationToken cancellationToken = default)
        {
            await SendAsync(cancellationToken, 0x81, 0x01, 0x04, 0x00, 0x02, 0xff).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            // TODO: Make this actually work...
            int attemptsLeft = 30;
            while (true)
            {
                try
                {
                    await GetPowerStatus().ConfigureAwait(false);
                    return;
                }
                catch (Exception)
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    attemptsLeft--;
                    if (attemptsLeft == 0)
                    {
                        throw;
                    }
                }
            }
        }

        public async Task PowerOff(CancellationToken cancellationToken = default) =>
            await SendAsync(cancellationToken, 0x81, 0x01, 0x04, 0x00, 0x03, 0xff).ConfigureAwait(false);

        public void Dispose() => client.Dispose();

        public async Task<PowerStatus> GetPowerStatus(CancellationToken cancellationToken = default)
        {
            var response = await SendAsync(cancellationToken, 0x81, 0x09, 0x04, 0x00, 0xff).ConfigureAwait(false);
            return (PowerStatus) response.GetByte(2);
        }

        public async Task<short> GetZoom(CancellationToken cancellationToken = default)
        {
            var response = await SendAsync(cancellationToken, 0x81, 0x09, 0x04, 0x47, 0xff).ConfigureAwait(false);
            return response.GetInt16(2);
        }

        public async Task<(short pan, short tilt)> GetPanTilt(CancellationToken cancellationToken = default)
        {
            var response = await SendAsync(cancellationToken, 0x81, 0x09, 0x06, 0x12, 0xff).ConfigureAwait(false);
            return (response.GetInt16(2), response.GetInt16(6));
        }

        public async Task SetZoom(short zoom, CancellationToken cancellationToken = default)
        {
            byte[] bytes = new byte[] { 0x81, 0x01, 0x04, 0x47, 0, 0, 0, 0, 0xff };
            SetInt16(bytes, 4, zoom);
            await SendAsync(cancellationToken, bytes).ConfigureAwait(false);
        }

        public async Task ContinuousPanTilt(bool? pan, bool? tilt, byte panSpeed, byte tiltSpeed, CancellationToken cancellationToken = default)
        {
            byte actualPan = (byte) (pan is null ? 3 : pan.Value ? 2 : 1);
            byte actualTilt = (byte) (tilt is null ? 3 : tilt.Value ? 2 : 1);
            byte[] bytes = new byte[] { 0x81, 0x01, 0x06, 0x1, panSpeed, tiltSpeed, actualPan, actualTilt, 0xff };
            await SendAsync(cancellationToken, bytes).ConfigureAwait(false);
        }

        public async Task ContinuousZoom(bool? zoomIn, byte zoomSpeed, CancellationToken cancellationToken = default)
        {
            byte parameter = (byte) (zoomIn is null ? 0 : zoomIn.Value ? 0x20 | zoomSpeed : 0x30 | zoomSpeed);
            await SendAsync(cancellationToken, 0x81, 0x01, 0x04, 0x07, parameter, 0xff).ConfigureAwait(false);
        }

        public async Task SetPanTilt(short pan, short tilt, byte panSpeed, byte tiltSpeed, CancellationToken cancellationToken = default)
        {
            byte[] bytes = new byte[] { 0x81, 0x01, 0x06, 0x02, panSpeed, tiltSpeed, 0, 0, 0, 0, 0, 0, 0, 0, 0xff };
            SetInt16(bytes, 6, pan);
            SetInt16(bytes, 10, tilt);
            await SendAsync(cancellationToken, bytes).ConfigureAwait(false);
        }

        public async Task RelativePanTilt(short pan, short tilt, byte panSpeed, byte tiltSpeed, CancellationToken cancellationToken = default)
        {
            byte[] bytes = new byte[] { 0x81, 0x01, 0x06, 0x03, panSpeed, tiltSpeed, 0, 0, 0, 0, 0, 0, 0, 0, 0xff };
            SetInt16(bytes, 6, pan);
            SetInt16(bytes, 10, tilt);
            await SendAsync(cancellationToken, bytes).ConfigureAwait(false);
        }

        private void SetInt16(byte[] buffer, int start, short value)
        {
            buffer[start] = (byte) ((value >> 12) & 0xf);
            buffer[start + 1] = (byte) ((value >> 8) & 0xf);
            buffer[start + 2] = (byte) ((value >> 4) & 0xf);
            buffer[start + 3] = (byte) ((value >> 0) & 0xf);
        }

        public async Task GoHome(CancellationToken cancellationToken = default) =>
            await SendAsync(cancellationToken, 0x81, 0x01, 0x06, 0x04, 0xff).ConfigureAwait(false);

        private Task<ViscaPacket> SendAsync(CancellationToken cancellationToken, params byte[] packet)
        {
            var effectiveToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, new CancellationTokenSource(CommandTimeout).Token).Token;
            var request = ViscaPacket.FromBytes(packet, 0, packet.Length);
            return client.SendAsync(request, effectiveToken);
        }
    }
}
