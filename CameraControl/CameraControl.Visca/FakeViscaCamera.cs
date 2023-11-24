// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;

namespace CameraControl.Visca;

/// <summary>
/// A VISCA client (to be used with <see cref="ViscaController"/>) that is purely fake,
/// always responding with success messages (unless bad input is detected), and faking the ability
/// to pan, tilt and zoom within the same limits as PTZOptics PT30X-NDI cameras. 
/// </summary>
/// <remarks>
/// Starting the camera creates a new background thread. Disposing of it stops this thread. Note
/// that disposing of the controller also disposes this.
/// </remarks>
public sealed class FakeViscaCamera : IDisposable
{
    private bool disposed;

    // Panning at maximum speed from hard-left to hard-right should take 4-5 seconds.
    // Distance travelled = MaxPan-MinPan = 4896
    // Steps at max speed = 4896 / MaxPanSpeed = 204
    // At 20ms, that's 4080ms - that'll do.
    private static readonly TimeSpan DelayStep = TimeSpan.FromMilliseconds(20);

    // Zooming at maximum speed from reset to "fully in" or vice versa should take 4-5 seconds.
    // Single steps at max speed = 16384 / MaxZoomSpeed = 2340
    // Time for 2340 single steps = 46800
    // With a multiplier of 10, the time is 4680ms - that'll do.
    private const int ZoomSpeedMultiplier = 10;

    private const short MinPan = -2448;
    private const short MaxPan = 2448;

    private const short MinTilt = -432;
    private const short MaxTilt = 1296;

    private const short MinZoom = 0;
    private const short MaxZoom = 16384;

    // TODO: Make these threadsafe, given that we really are accessing them from all over the place.
    public short Zoom { get; private set; }
    public short Pan { get; private set; }
    public short Tilt { get; private set; }

    private volatile sbyte zoomVelocity;
    private volatile sbyte panVelocity;
    private volatile sbyte tiltVelocity;

    private FakeViscaCamera()
    {
    }

    public ViscaController CreateController(TimeSpan? commandTimeout, ILogger? logger) =>
        new ViscaController(new FakeViscaClient(this), commandTimeout ?? ViscaController.DefaultTimeout, logger);

    public static FakeViscaCamera Start()
    {
        var ret = new FakeViscaCamera();
        new Thread(ret.Loop) { IsBackground = true }.Start();
        return ret;
    }

    private void Loop()
    {
        while (!disposed)
        {
            if (panVelocity != 0)
            {
                Pan = Math.Min(Math.Max((short) (Pan + panVelocity), MinPan), MaxPan);
                if (Pan == MinPan || Pan == MaxPan)
                {
                    panVelocity = 0;
                }
            }
            if (tiltVelocity != 0)
            {
                Tilt = Math.Min(Math.Max((short) (Tilt + tiltVelocity), MinTilt), MaxTilt);
                if (Tilt == MinTilt || Tilt == MaxTilt)
                {
                    tiltVelocity = 0;
                }
            }
            if (zoomVelocity != 0)
            {
                Zoom = Math.Min(Math.Max((short) (Zoom + (zoomVelocity * ZoomSpeedMultiplier)), MinZoom), MaxZoom);
                if (Zoom == MinZoom || Zoom == MaxZoom)
                {
                    zoomVelocity = 0;
                }
            }
            Thread.Sleep(DelayStep);
        }
    }

    public void Dispose() => disposed = true;

    /// <summary>
    /// A Visca client for the fake camera. The behaviour of multiple clients for the same camera is somewhat
    /// unpredictable if they're both trying to change pan/tilt etc, but most other operations should be okay
    /// - in particular, it's fine to have one client polling the power status, effectively as a connection check,
    /// while another operates the fake camera normally.
    /// </summary>
    private class FakeViscaClient : IViscaClient
    {
        // Note: all these assume a camera address of 1, which corresponds to all the VISCA-over-IP commands.
        private static readonly ViscaPayload SyntaxError = CreatePayload(0x90, 0x60, 0x02);
        private static readonly ViscaPayload CommandNotExecutable = CreatePayload(0x90, 0x60, 0x41);
        private static readonly ViscaPayload Done = CreatePayload(0x90, 0x50);
        private static readonly byte[] QueryResponsePrefix = { 0x90, 0x50 };

        private readonly FakeViscaCamera camera;

        public void Dispose() { }

        internal FakeViscaClient(FakeViscaCamera camera)
        {
            this.camera = camera;
        }

        // We only allow one command to execute at a time per controller. That makes life a lot simpler.
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        async Task<ViscaMessage> IViscaClient.SendAsync(ViscaMessage message, CancellationToken cancellationToken)
        {
            var payload = message.Payload;
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (payload.GetByte(0) != 0x81 || payload.Length < 5)
                {
                    return new ViscaMessage(SyntaxError);
                }
                var responsePayload = payload.GetByte(1) switch
                {
                    0x01 => await ExecuteCommand(payload).ConfigureAwait(false),
                    0x09 => ExecuteQuery(payload),
                    _ => SyntaxError
                };
                return new ViscaMessage(responsePayload);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private Task<ViscaPayload> ExecuteCommand(ViscaPayload request) => request.GetByte(2) switch
        {
            0x04 => ExecuteGeneralCommand(request),
            0x06 => ExecutePanTiltDriveCommand(request),
            _ => Task.FromResult(CommandNotExecutable)
        };

        /// <summary>
        /// Executes a command beginning with 0x81 0x01 0x04.
        /// </summary>
        private async Task<ViscaPayload> ExecuteGeneralCommand(ViscaPayload request)
        {
            switch (request.GetByte(3))
            {
                // CAM_Zoom
                case 0x07:
                    byte arg = request.GetByte(4);
                    switch (arg)
                    {
                        // Stop
                        case 0x00:
                            camera.zoomVelocity = 0;
                            break;
                        case 0x02:
                            camera.zoomVelocity = (sbyte) ViscaController.MaxZoomSpeed;
                            break;
                        case 0x03:
                            camera.zoomVelocity = -(sbyte) ViscaController.MaxZoomSpeed;
                            break;
                        // TODO: Handle Stop / tele standard / wide standard
                        case >= 0x20 and < 0x30:
                            camera.zoomVelocity = (sbyte) (arg & 0x0f);
                            break;
                        case >= 0x30 and < 0x40:
                            camera.zoomVelocity = (sbyte) -(arg & 0x0f);
                            break;
                        default:
                            return CommandNotExecutable;
                    }
                    break;
                // Direct zoom
                case 0x47:
                    await MoveZoom(request.GetInt16(4), ViscaController.MaxZoomSpeed).ConfigureAwait(false);
                    break;
                // Power
                // TODO: simulate power off etc. For the moment, just say "okay" regardless...
                case 0x00:
                    break;
                default:
                    // TODO: Implement or at least say "okay"
                    return CommandNotExecutable;
            }
            return Done;
        }

        /// <summary>
        /// Executes a command beginning with 0x81 0x01 0x06
        /// </summary>
        private async Task<ViscaPayload> ExecutePanTiltDriveCommand(ViscaPayload request)
        {
            switch (request.GetByte(3))
            {
                // Continuous pan/tilt
                case 0x01:
                    var panSpeed = request.GetByte(4);
                    var tiltSpeed = request.GetByte(5);
                    var panSign = ConvertSign(request.GetByte(6));
                    var tiltSign = ConvertSign(request.GetByte(7)) * -1; // Pan has "2 for positive, 1 for negative"; tilt has the reverse :(
                    camera.panVelocity = (sbyte) (panSpeed * panSign);
                    camera.tiltVelocity = (sbyte) (tiltSpeed * tiltSign);
                    break;
                // Absolute position
                // TODO: check request length, validate parameters
                case 0x02:
                    await MovePanTilt(request.GetInt16(6), request.GetInt16(10), request.GetByte(4), request.GetByte(5)).ConfigureAwait(false);
                    break;
                // Relative position
                // TODO: check request length, validate parameters
                case 0x03:
                    await MovePanTilt((short) (camera.Pan + request.GetInt16(6)), (short) (camera.Tilt + request.GetInt16(10)), request.GetByte(4), request.GetByte(5)).ConfigureAwait(false);
                    break;
                // Home / reset
                // TODO: Reset zoom as well
                case 0x04:
                case 0x05:
                    await MovePanTilt(0, 0, ViscaController.MaxPanTiltSpeed, ViscaController.MaxPanTiltSpeed).ConfigureAwait(false);
                    await MoveZoom(0, ViscaController.MaxZoomSpeed).ConfigureAwait(false);
                    break;
                default:
                    return CommandNotExecutable;
            };
            return Done;

            int ConvertSign(byte b) => b switch
            {
                1 => -1,
                2 => 1,
                3 => 0,
                _ => 0 // But log?
            };
        }

        private async Task MovePanTilt(short pan, short tilt, byte panSpeed, byte tiltSpeed)
        {
            // Stop the normal thread from updating anything.
            camera.panVelocity = 0;
            camera.zoomVelocity = 0;

            panSpeed = Math.Max(panSpeed, (byte) 1);
            tiltSpeed = Math.Max(tiltSpeed, (byte) 1);
            while (camera.Pan != pan || camera.Tilt != tilt)
            {
                camera.Pan = MoveTowardsTarget(camera.Pan, pan, panSpeed);
                camera.Tilt = MoveTowardsTarget(camera.Tilt, tilt, tiltSpeed);
                await Task.Delay(DelayStep).ConfigureAwait(false);
            }
        }

        private async Task MoveZoom(short zoom, byte zoomSpeed)
        {
            zoomSpeed = Math.Max(zoomSpeed, (byte) 1);
            while (camera.Zoom != zoom)
            {
                camera.Zoom = MoveTowardsTarget(camera.Zoom, zoom, (byte) (zoomSpeed * ZoomSpeedMultiplier));
                await Task.Delay(DelayStep).ConfigureAwait(false);
            }
        }

        private short MoveTowardsTarget(short current, short target, byte maxChange) =>
            current < target
            ? Math.Min((short) (current + maxChange), target)
            : Math.Max((short) (current - maxChange), target);

        private ViscaPayload ExecuteQuery(ViscaPayload request)
        {
            if (request.Length != 5 || request.GetByte(4) != 0xff)
            {
                return SyntaxError;
            }
            return (request.GetByte(2), request.GetByte(3)) switch
            {
                // Zoom position
                (0x04, 0x47) => Int16QueryResponse(camera.Zoom),
                // Pan/tilt
                (0x06, 0x12) => Int16Int16QueryResponse(camera.Pan, camera.Tilt),
                // Power (always "on" at the moment...)
                (0x04, 0x00) => QueryResponse(0x02),
                _ => CommandNotExecutable
            };

            ViscaPayload Int16QueryResponse(short value)
            {
                byte[] data = new byte[4];
                SetInt16(data, 0, value);
                return QueryResponse(data);
            }

            ViscaPayload Int16Int16QueryResponse(short value1, short value2)
            {
                byte[] data = new byte[8];
                SetInt16(data, 0, value1);
                SetInt16(data, 4, value2);
                return QueryResponse(data);
            }

            ViscaPayload QueryResponse(params byte[] data) =>
                CreatePacket(QueryResponsePrefix, data);

            void SetInt16(byte[] buffer, int start, short value)
            {
                buffer[start] = (byte) ((value >> 12) & 0xf);
                buffer[start + 1] = (byte) ((value >> 8) & 0xf);
                buffer[start + 2] = (byte) ((value >> 4) & 0xf);
                buffer[start + 3] = (byte) ((value >> 0) & 0xf);
            }
        }

        private static ViscaPayload CreatePacket(byte[] prefix, params byte[] data) =>
            CreatePayload(prefix.Concat(data).ToArray());

        /// <summary>
        /// Creates a packet from the given byte array, which is *not* expected to
        /// include the trailing FF (just for convenience).
        /// </summary>
        private static ViscaPayload CreatePayload(params byte[] bytes)
        {
            var buffer = new byte[bytes.Length + 1];
            Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);
            buffer[bytes.Length] = 0xff;
            return ViscaPayload.FromBytes(buffer);
        }
    }
}
