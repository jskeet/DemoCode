// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace CameraControl.Visca
{
    /// <summary>
    /// Base class for VISCA clients, handling VISCA protocol violations and reconnections.
    /// </summary>
    internal abstract class ViscaClientBase : IViscaClient
    {
        private readonly SemaphoreSlim semaphore = new(1);

        protected ILogger? Logger { get; }

        /// <summary>
        /// Sends a single packet to the VISCA endpoint.
        /// </summary>
        /// <param name="packet">The packet to send to the endpoint.</param>
        /// <param name="cancellationToken">A token to indicate that the method has been cancelled.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected abstract Task SendPacketAsync(ViscaPacket packet, CancellationToken cancellationToken);

        /// <summary>
        /// Receives a packet from the VISCA endpoint.
        /// </summary>
        /// <param name="cancellationToken">A token to indicate that the method has been cancelled.</param>
        /// <returns>The packet received from the endpoint.</returns>
        /// <exception cref="ViscaProtocolException">The endpoint violated the VISCA protocol.</exception>
        protected abstract Task<ViscaPacket> ReceivePacketAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Reconnects the client either initially, or after an error.
        /// </summary>
        /// <param name="cancellationToken">A token to indicate that the method has been cancelled.</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected abstract Task ReconnectAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects in a way that will force a reconnect on the next request.
        /// </summary>
        protected abstract void Disconnect();

        public abstract void Dispose();

        protected ViscaClientBase(ILogger? logger)
        {
            Logger = logger;
        }

        async Task<ViscaPacket> IViscaClient.SendAsync(ViscaPacket request, CancellationToken cancellationToken)
        {
            bool disconnect = true;
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await SendPacketAsync(request, cancellationToken).ConfigureAwait(false);
                while (true)
                {
                    ViscaPacket response = await ReceivePacketAsync(cancellationToken).ConfigureAwait(false);
                    Logger?.LogTrace("Received VISCA packet: {packet}", response);
                    if (response.Length < 2)
                    {
                        throw new ViscaProtocolException($"Received packet of length {response.Length} from VISCA endpoint");
                    }
                    switch (response.GetByte(1) >> 4)
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
}
