// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CameraControl.Visca;

public sealed class ViscaController : IDisposable
{
    private static readonly ViscaMessage PowerOnMessage = new ViscaMessage(ViscaMessageType.Command, 0x81, 0x01, 0x04, 0x00, 0x02, 0xff);
    private static readonly ViscaMessage PowerOffMessage = new ViscaMessage(ViscaMessageType.Command, 0x81, 0x01, 0x04, 0x00, 0x03, 0xff);
    private static readonly ViscaMessage GetPowerStatusMessage = new ViscaMessage(ViscaMessageType.Inquiry, 0x81, 0x09, 0x04, 0x00, 0xff);
    private static readonly ViscaMessage GetZoomMessage = new ViscaMessage(ViscaMessageType.Inquiry, 0x81, 0x09, 0x04, 0x47, 0xff);
    private static readonly ViscaMessage GetPanTiltMessage = new ViscaMessage(ViscaMessageType.Inquiry, 0x81, 0x09, 0x06, 0x12, 0xff);
    private static readonly ViscaMessage GoHomeMessage = new ViscaMessage(ViscaMessageType.Command, 0x81, 0x01, 0x06, 0x04, 0xff);

    private static readonly ViscaPayload SetZoomTemplate = ViscaPayload.FromBytes(0x81, 0x01, 0x04, 0x47, 0, 0, 0, 0, 0xff);
    private static readonly ViscaPayload ContinuousPanTiltTemplate = ViscaPayload.FromBytes(0x81, 0x01, 0x06, 0x1, 0, 0, 0, 0, 0xff);
    private static readonly ViscaPayload ContinuousZoomTemplate = ViscaPayload.FromBytes(0x81, 0x01, 0x04, 0x07, 0, 0xff);
    private static readonly ViscaPayload SetPanTiltTemplate = ViscaPayload.FromBytes(0x81, 0x01, 0x06, 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xff);
    private static readonly ViscaPayload RelativePanTiltTemplate = ViscaPayload.FromBytes(0x81, 0x01, 0x06, 0x03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xff);

    internal static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(15);

    public const byte MinSpeed = 0x01;
    public const byte MaxPanTiltSpeed = 0x18;
    public const byte MaxZoomSpeed = 0x7;

    private TimeSpan CommandTimeout { get; }
    private readonly ILogger? logger;
    private readonly IViscaClient client;
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();

    internal ViscaController(IViscaClient client, TimeSpan commandTimeout, ILogger? logger)
    {
        this.client = client;
        this.CommandTimeout = commandTimeout;
        this.logger = logger;
    }

    public static ViscaController ForTcp(string host, int port, ViscaMessageFormat format, TimeSpan? commandTimeout = null, ILogger? logger = null, TcpSendLock? sendLock = null)
    {
        var client = new TcpViscaClient(host, port, format, logger, sendLock);
        return new ViscaController(client, commandTimeout ?? DefaultTimeout, logger);
    }

    public static ViscaController ForUdp(string host, int port, ViscaMessageFormat format, TimeSpan? commandTimeout = null, ILogger? logger = null)
    {
        var client = new UdpViscaClient(host, port, format, logger);
        return new ViscaController(client, commandTimeout ?? DefaultTimeout, logger);
    }

    public async Task PowerCycle(CancellationToken cancellationToken = default)
    {
        await PowerOff(cancellationToken).ConfigureAwait(false);
        // On Minrray cameras, connections fail immediately after PowerOff, so we retry here for a bit.
        await RetryWithConstantBackoff(async token => { await PowerOn(token).ConfigureAwait(false); return 0; },
            attempts: 10, perOperationTimeout: TimeSpan.FromSeconds(1), delay: TimeSpan.FromSeconds(1));

        // Keep trying to get the power status until we can actually do so. This can take a few attempts.
        // Note that we don't actually try to validate that the power status is "on".
        var status = await RetryWithConstantBackoff(GetPowerStatus, attempts: 20, perOperationTimeout: TimeSpan.FromSeconds(1), delay: TimeSpan.FromSeconds(2));
        logger?.LogDebug("Power cycle complete; power status: {status}", status);

        async Task<T> RetryWithConstantBackoff<T>(Func<CancellationToken, Task<T>> operation, int attempts, TimeSpan perOperationTimeout, TimeSpan delay)
        {
            int attemptsLeft = attempts;
            while (true)
            {
                try
                {
                    var shortCancellationToken = new CancellationTokenSource(perOperationTimeout).Token;
                    var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, shortCancellationToken).Token;
                    var result = await operation(linkedToken).ConfigureAwait(false);
                    return result;
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested && attemptsLeft > 0)
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    attemptsLeft--;
                }
            }
        }
    }

    public async Task PowerOn(CancellationToken cancellationToken = default) =>
        await SendAsync(PowerOnMessage, cancellationToken).ConfigureAwait(false);

    public async Task PowerOff(CancellationToken cancellationToken = default) =>
        await SendAsync(PowerOffMessage, cancellationToken).ConfigureAwait(false);

    public void Dispose() => client.Dispose();

    public async Task<PowerStatus> GetPowerStatus(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(GetPowerStatusMessage, cancellationToken).ConfigureAwait(false);
        return (PowerStatus) response.Payload.GetByte(2);
    }

    public async Task<short> GetZoom(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(GetZoomMessage, cancellationToken).ConfigureAwait(false);
        return response.Payload.GetInt16(2);
    }

    public async Task<(short pan, short tilt)> GetPanTilt(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(GetPanTiltMessage, cancellationToken).ConfigureAwait(false);
        return (response.Payload.GetInt16(2), response.Payload.GetInt16(6));
    }

    // TODO: What about 41, 42 etc? Would that be "direct with variable speed"?
    public async Task SetZoom(short zoom, CancellationToken cancellationToken = default)
    {
        var message = SetZoomTemplate.WithInt16Set(4, zoom).ToCommandMessage();
        await SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Changes the current rate of pan/tilt.
    /// </summary>
    /// <param name="pan">true to pan right (positive); false to pan left (negative); null for no pan</param>
    /// <param name="tilt">true to tilt up (positive); false to tilt down (negative); null for no tilt</param>
    /// <param name="panSpeed">The speed at which to pan</param>
    /// <param name="tiltSpeed">The speed at which to tilt</param>
    /// <param name="cancellationToken">A cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task ContinuousPanTilt(bool? pan, bool? tilt, byte panSpeed, byte tiltSpeed, CancellationToken cancellationToken = default)
    {
        byte actualPan = (byte) (pan is null ? 3 : pan.Value ? 2 : 1);
        byte actualTilt = (byte) (tilt is null ? 3 : tilt.Value ? 1 : 2);
        var message = ContinuousPanTiltTemplate
            .WithByteSet(4, panSpeed)
            .WithByteSet(5, tiltSpeed)
            .WithByteSet(6, actualPan)
            .WithByteSet(7, actualTilt)
            .ToCommandMessage();
        await SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    public async Task ContinuousZoom(bool? zoomIn, byte zoomSpeed, CancellationToken cancellationToken = default)
    {
        byte parameter = (byte) (zoomIn is null ? 0 : zoomIn.Value ? 0x20 | zoomSpeed : 0x30 | zoomSpeed);
        var message = ContinuousZoomTemplate.WithByteSet(4, parameter).ToCommandMessage();
        await SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    public async Task SetPanTilt(short pan, short tilt, byte panSpeed, byte tiltSpeed, CancellationToken cancellationToken = default)
    {
        var message = SetPanTiltTemplate
            .WithByteSet(4, panSpeed)
            .WithByteSet(5, tiltSpeed)
            .WithInt16Set(6, pan)
            .WithInt16Set(10, tilt)
            .ToCommandMessage();
        await SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    public async Task RelativePanTilt(short pan, short tilt, byte panSpeed, byte tiltSpeed, CancellationToken cancellationToken = default)
    {
        var message = RelativePanTiltTemplate
            .WithByteSet(4, panSpeed)
            .WithByteSet(5, tiltSpeed)
            .WithInt16Set(6, pan)
            .WithInt16Set(10, tilt)
            .ToCommandMessage();
        await SendAsync(message, cancellationToken).ConfigureAwait(false);
    }

    public async Task GoHome(CancellationToken cancellationToken = default) =>
        await SendAsync(GoHomeMessage, cancellationToken).ConfigureAwait(false);

    private async Task<ViscaMessage> SendAsync(ViscaMessage message, CancellationToken cancellationToken, [CallerMemberName] string? command = null)
    {
        var effectiveToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, new CancellationTokenSource(CommandTimeout).Token).Token;
        logger?.LogDebug("Sending VISCA command '{command}': {request}", command, message.Payload);
        long ticksBefore = stopwatch.ElapsedTicks;
        var response = await client.SendAsync(message, effectiveToken).ConfigureAwait(false);
        long ticksAfter = stopwatch.ElapsedTicks;
        logger?.LogDebug("VISCA command '{command}' completed in {millis}ms", command, (ticksAfter - ticksBefore) * 1000 / Stopwatch.Frequency);
        return response;
    }
}
