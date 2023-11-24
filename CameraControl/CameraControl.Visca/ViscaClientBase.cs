// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;

namespace CameraControl.Visca;

/// <summary>
/// Base class for VISCA clients, handling VISCA protocol violations and reconnections.
/// </summary>
internal abstract class ViscaClientBase : IViscaClient
{
    private readonly SemaphoreSlim semaphore = new(1);

    protected ViscaMessageFormat Format { get; }

    protected ILogger? Logger { get; }

    private int? sequenceNumber;

    /// <summary>
    /// Resets internal state owned by the base class - typically sequence numbers, when they're being used.
    /// </summary>
    protected void ResetState()
    {
    }

    /// <summary>
    /// Sends a single message to the VISCA endpoint.
    /// </summary>
    /// <param name="message">The message to send to the endpoint.</param>
    /// <param name="cancellationToken">A token to indicate that the method has been cancelled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task SendMessageAsync(ViscaMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Receives a message from the VISCA endpoint.
    /// </summary>
    /// <param name="cancellationToken">A token to indicate that the method has been cancelled.</param>
    /// <returns>The message received from the endpoint.</returns>
    /// <exception cref="ViscaProtocolException">The endpoint violated the VISCA protocol.</exception>
    protected abstract Task<ViscaMessage> ReceiveMessageAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Disconnects in a way that will force a reconnect on the next request.
    /// </summary>
    protected abstract void Disconnect();

    public abstract void Dispose();

    protected ViscaClientBase(ViscaMessageFormat format, ILogger? logger)
    {
        sequenceNumber = format == ViscaMessageFormat.Raw ? null : 0;
        Format = format;
        Logger = logger;
    }

    async Task<ViscaMessage> IViscaClient.SendAsync(ViscaMessage request, CancellationToken cancellationToken)
    {
        bool disconnect = true;
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (sequenceNumber is not null)
            {
                request = request with { SequenceNumber = sequenceNumber++ };
            }

            await SendMessageAsync(request, cancellationToken).ConfigureAwait(false);
            while (true)
            {
                ViscaMessage response = await ReceiveMessageAsync(cancellationToken).ConfigureAwait(false);
                var responsePayload = response.Payload;
                Logger?.LogTrace("Received VISCA packet: {packet}", response);
                if (responsePayload.Length < 2)
                {
                    throw new ViscaProtocolException($"Received payload of length {responsePayload.Length} from VISCA endpoint");
                }
                switch (responsePayload.GetByte(1) >> 4)
                {
                    // Command received
                    case 4:
                        continue;
                    // Success
                    case 5:
                        disconnect = false;
                        return response;
                    // Error reported by device
                    case 6:
                        throw new ViscaResponseException($"Error reported by VISCA interface. Error data: {response}");
                    // VISCA protocol violation
                    default:
                        throw new ViscaProtocolException($"Invalid packet returned from VISCA interface. Error data: {response}");
                }
            }
        }
        finally
        {
            try
            {
                if (disconnect)
                {
                    // We can't reconnect here, as that could take time and the cancellation token
                    // may already have been cancelled.
                    Disconnect();
                }
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
